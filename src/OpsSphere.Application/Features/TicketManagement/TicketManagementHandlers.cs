using System.Text.Json;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Authorization;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Exceptions;
using OpsSphere.Domain.Services;

namespace OpsSphere.Application.Features.TicketManagement;

public sealed class CreateTicketCommandHandler(
    ITicketRepository repository,
    IScopeAuthorizationService scopeAuthorization,
    ICurrentUserContext currentUser,
    IAuditWriter auditWriter,
    ITicketNumberGenerator ticketNumberGenerator)
{
    public async Task<CreateTicketResult> HandleAsync(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var (category, subject, description, priority) = TicketValidation.Validate(
            command.CustomerId, command.AccountId, command.CampaignId,
            command.Category, command.Subject, command.Description, command.Priority);

        var access = await scopeAuthorization.CanAccessCampaignAsync(command.CampaignId, cancellationToken);
        if (!access.IsAllowed)
            throw new NotFoundException("Campaign", command.CampaignId);

        var campaignSnapshot = await repository.GetCampaignSnapshotAsync(command.CampaignId, cancellationToken)
            ?? throw new NotFoundException("Campaign", command.CampaignId);
        if (!campaignSnapshot.IsActive)
            throw new ValidationException("campaignId", "Campaign must be active.");
        if (campaignSnapshot.AccountId != command.AccountId)
            throw new ValidationException("accountId", "Account does not belong to the specified campaign.");

        var customerSnapshot = await repository.GetCustomerSnapshotAsync(command.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Customer", command.CustomerId);
        if (!customerSnapshot.IsActive)
            throw new ValidationException("customerId", "Customer must be active.");
        if (customerSnapshot.AccountId != campaignSnapshot.AccountId)
            throw new ValidationException("customerId", "Customer does not belong to the campaign's account.");

        var ticketNumber = await ticketNumberGenerator.GenerateAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var slaDueAt = now.Add(TicketSlaConstants.GetSlaDuration(priority));
        var createdByUserId = currentUser.UserId ?? throw new ValidationException("userId", "Authenticated user context is required.");

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            CustomerId = command.CustomerId,
            RegionId = campaignSnapshot.RegionId,
            CountryId = campaignSnapshot.CountryId,
            AccountId = command.AccountId,
            CampaignId = command.CampaignId,
            CreatedByUserId = createdByUserId,
            Category = category,
            Priority = priority,
            Status = TicketStatus.Open,
            Subject = subject,
            Description = description,
            SlaState = SlaState.WithinSla,
            SlaDueAt = slaDueAt,
            IsEscalated = false,
            IsDeleted = false,
            CreatedAt = now
        };

        var statusHistory = new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PreviousStatus = null,
            NewStatus = TicketStatus.Open.ToString(),
            ChangedByUserId = createdByUserId,
            ChangeReason = "Ticket created",
            CreatedAt = now
        };

        var slaState = new TicketSlaState
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            SlaPolicyId = null,
            StartedAt = now,
            DueAt = slaDueAt,
            State = SlaState.WithinSla.ToString()
        };

        await repository.AddTicketAsync(ticket, statusHistory, slaState, cancellationToken);

        var auditPayload = TicketAuditJson.Serialize(new
        {
            ticket.TicketNumber,
            ticket.CustomerId,
            ticket.AccountId,
            ticket.CampaignId,
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status.ToString(),
            ticket.Category,
            SlaState = ticket.SlaState.ToString()
        });
        await auditWriter.WriteAsync("TicketCreated", "Ticket", ticket.Id, null, auditPayload, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return new CreateTicketResult(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.SlaState.ToString(),
            ticket.SlaDueAt);
    }
}

public sealed class GetTicketsQueryHandler(ITicketRepository repository)
{
    public Task<IReadOnlyList<TicketListItemDto>> HandleAsync(GetTicketsQuery query, CancellationToken cancellationToken) =>
        repository.GetTicketsAsync(cancellationToken);
}

public sealed class GetTicketByIdQueryHandler(ITicketRepository repository)
{
    public async Task<TicketDetailDto> HandleAsync(GetTicketByIdQuery query, CancellationToken cancellationToken) =>
        await repository.GetTicketByIdAsync(query.Id, cancellationToken) ?? throw new NotFoundException("Ticket", query.Id);
}

public sealed class AssignTicketCommandHandler(
    ITicketRepository repository,
    ICurrentUserContext currentUser,
    IAuditWriter auditWriter)
{
    private const int AssignmentReasonMaxLength = 500;

    public async Task<AssignTicketResponse> HandleAsync(AssignTicketCommand command, CancellationToken cancellationToken)
    {
        var reassignmentReason = Validate(command);

        var ticket = await repository.GetTicketForAssignmentAsync(command.TicketId, cancellationToken)
            ?? throw new NotFoundException("Ticket", command.TicketId);

        EnsureTicketCanBeAssigned(ticket.Status);

        if (ticket.AssignedAgentUserId == command.TargetAgentUserId)
            throw new BusinessRuleException("Ticket is already assigned to the selected agent.");

        var candidate = await repository.GetAgentAssignmentCandidateAsync(command.TargetAgentUserId, cancellationToken)
            ?? throw new NotFoundException("User", command.TargetAgentUserId);

        ValidateCandidate(candidate, ticket);

        var actorUserId = currentUser.UserId ?? throw new ValidationException("userId", "Authenticated user context is required.");
        var previousStatus = ticket.Status;
        var previousAgentUserId = ticket.AssignedAgentUserId;
        var now = DateTime.UtcNow;

        ticket.AssignedAgentUserId = command.TargetAgentUserId;
        ticket.UpdatedAt = now;

        if (ticket.Status == TicketStatus.Open)
        {
            ticket.Status = TicketStatus.Assigned;
            await repository.AddStatusHistoryAsync(new TicketStatusHistory
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                PreviousStatus = TicketStatus.Open.ToString(),
                NewStatus = TicketStatus.Assigned.ToString(),
                ChangedByUserId = actorUserId,
                ChangeReason = "Ticket assigned",
                CreatedAt = now
            }, cancellationToken);
        }

        await repository.AddAssignmentAsync(new TicketAssignment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PreviousAgentUserId = previousAgentUserId,
            NewAgentUserId = command.TargetAgentUserId,
            AssignedByUserId = actorUserId,
            AssignmentReason = reassignmentReason,
            CreatedAt = now
        }, cancellationToken);

        await auditWriter.WriteAsync(
            "TicketAssigned",
            "Ticket",
            ticket.Id,
            TicketAuditJson.Serialize(new
            {
                assignedAgentUserId = previousAgentUserId,
                status = previousStatus.ToString()
            }),
            TicketAuditJson.Serialize(new
            {
                assignedAgentUserId = command.TargetAgentUserId,
                status = ticket.Status.ToString()
            }),
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return new AssignTicketResponse(
            ticket.Id,
            ticket.TicketNumber,
            command.TargetAgentUserId,
            previousAgentUserId,
            ticket.Status.ToString(),
            "Ticket assigned successfully.");
    }

    private static string? Validate(AssignTicketCommand command)
    {
        var failures = new List<ValidationFailure>();
        if (command.TicketId == Guid.Empty) failures.Add(new ValidationFailure("ticketId", "Ticket is required."));
        if (command.TargetAgentUserId == Guid.Empty) failures.Add(new ValidationFailure("targetAgentUserId", "Target agent is required."));

        var reassignmentReason = string.IsNullOrWhiteSpace(command.ReassignmentReason)
            ? null
            : command.ReassignmentReason.Trim();
        if (reassignmentReason?.Length > AssignmentReasonMaxLength)
            failures.Add(new ValidationFailure("reassignmentReason", $"Reassignment reason must be {AssignmentReasonMaxLength} characters or fewer."));

        if (failures.Count > 0) throw new ValidationException(failures);
        return reassignmentReason;
    }

    private static void EnsureTicketCanBeAssigned(TicketStatus status)
    {
        try
        {
            TicketWorkflowRules.EnsureCanAssign(status);
        }
        catch (TicketDomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    private static void ValidateCandidate(AgentAssignmentCandidateSnapshot candidate, Ticket ticket)
    {
        if (!candidate.IsActive)
            throw new ValidationException("targetAgentUserId", "Target agent must be active.");

        if (!candidate.HasAgentRole)
            throw new ValidationException("targetAgentUserId", "Target user is not eligible for ticket assignment.");

        if (!IsEligibleForTicket(candidate, ticket))
            throw new ValidationException("targetAgentUserId", "Target agent is outside the ticket scope.");
    }

    private static bool IsEligibleForTicket(AgentAssignmentCandidateSnapshot candidate, Ticket ticket) =>
        candidate.ActiveScopes.Any(scope =>
            (scope.ScopeType == ScopeTypes.Account && scope.AccountId == ticket.AccountId) ||
            (scope.ScopeType == ScopeTypes.Campaign && scope.CampaignId == ticket.CampaignId));
}

public sealed class GetEligibleAgentsQueryHandler(ITicketRepository repository)
{
    public async Task<IReadOnlyList<EligibleAgentDto>> HandleAsync(GetEligibleAgentsQuery query, CancellationToken cancellationToken)
    {
        if (query.TicketId == Guid.Empty)
            throw new ValidationException("ticketId", "Ticket is required.");

        var ticket = await repository.GetTicketForAssignmentAsync(query.TicketId, cancellationToken)
            ?? throw new NotFoundException("Ticket", query.TicketId);

        try
        {
            TicketWorkflowRules.EnsureCanAssign(ticket.Status);
        }
        catch (TicketDomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        return await repository.GetEligibleAgentsAsync(ticket.CampaignId, ticket.AccountId, cancellationToken);
    }
}

public sealed class UpdateTicketStatusCommandHandler(
    ITicketRepository repository,
    ICurrentUserContext currentUser,
    IAuditWriter auditWriter)
{
    private const int ChangeReasonMaxLength = 500;
    private static readonly HashSet<TicketStatus> AllowedDestinationStatuses =
    [
        TicketStatus.Assigned,
        TicketStatus.InProgress,
        TicketStatus.WaitingForCustomer
    ];

    public async Task<UpdateTicketStatusResponse> HandleAsync(UpdateTicketStatusCommand command, CancellationToken cancellationToken)
    {
        var (requestedStatus, changeReason) = Validate(command);

        var ticket = await repository.GetTicketForAssignmentAsync(command.TicketId, cancellationToken)
            ?? throw new NotFoundException("Ticket", command.TicketId);

        EnsureTicketCanBeModified(ticket.Status);

        if (ticket.Status == requestedStatus)
            throw new BusinessRuleException("Ticket is already in this status.");

        if (!AllowedDestinationStatuses.Contains(requestedStatus))
            throw new BusinessRuleException(
                $"Cannot update ticket status to {requestedStatus}. Only Assigned, InProgress, and WaitingForCustomer are allowed in this workflow.");

        if (requestedStatus == TicketStatus.Assigned && ticket.AssignedAgentUserId is null)
            throw new BusinessRuleException("Cannot set status to Assigned when no agent is assigned.");

        EnsureValidTicketTransition(ticket.Status, requestedStatus);

        var actorUserId = currentUser.UserId ?? throw new ValidationException("userId", "Authenticated user context is required.");
        var previousStatus = ticket.Status;
        var now = DateTime.UtcNow;

        ticket.Status = requestedStatus;
        ticket.UpdatedAt = now;

        await repository.AddStatusHistoryAsync(new TicketStatusHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = requestedStatus.ToString(),
            ChangedByUserId = actorUserId,
            ChangeReason = changeReason ?? "Status updated",
            CreatedAt = now
        }, cancellationToken);

        await auditWriter.WriteAsync(
            "TicketStatusChanged",
            "Ticket",
            ticket.Id,
            TicketAuditJson.Serialize(new
            {
                status = previousStatus.ToString()
            }),
            TicketAuditJson.Serialize(new
            {
                status = requestedStatus.ToString()
            }),
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return new UpdateTicketStatusResponse(
            ticket.Id,
            ticket.TicketNumber,
            previousStatus.ToString(),
            requestedStatus.ToString(),
            "Ticket status updated successfully.");
    }

    private static (TicketStatus Status, string? ChangeReason) Validate(UpdateTicketStatusCommand command)
    {
        var failures = new List<ValidationFailure>();
        if (command.TicketId == Guid.Empty) failures.Add(new ValidationFailure("ticketId", "Ticket is required."));

        TicketStatus parsedStatus = default;
        if (string.IsNullOrWhiteSpace(command.Status))
            failures.Add(new ValidationFailure("status", "Status is required."));
        else if (!Enum.TryParse(command.Status.Trim(), ignoreCase: true, out parsedStatus))
            failures.Add(new ValidationFailure("status", $"Status must be one of: {string.Join(", ", Enum.GetNames<TicketStatus>())}."));

        var changeReason = string.IsNullOrWhiteSpace(command.ChangeReason)
            ? null
            : command.ChangeReason.Trim();
        if (changeReason?.Length > ChangeReasonMaxLength)
            failures.Add(new ValidationFailure("changeReason", $"Change reason must be {ChangeReasonMaxLength} characters or fewer."));

        if (failures.Count > 0) throw new ValidationException(failures);
        return (parsedStatus, changeReason);
    }

    private static void EnsureTicketCanBeModified(TicketStatus status)
    {
        try
        {
            TicketWorkflowRules.EnsureCanModify(status);
        }
        catch (TicketDomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }

    private static void EnsureValidTicketTransition(TicketStatus from, TicketStatus to)
    {
        try
        {
            TicketWorkflowRules.EnsureValidTransition(from, to);
        }
        catch (TicketDomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }
}

public sealed class UpdateTicketPriorityCommandHandler(
    ITicketRepository repository,
    IAuditWriter auditWriter)
{
    private const int ChangeReasonMaxLength = 500;

    public async Task<UpdateTicketPriorityResponse> HandleAsync(UpdateTicketPriorityCommand command, CancellationToken cancellationToken)
    {
        var (requestedPriority, _) = Validate(command);

        var ticket = await repository.GetTicketForAssignmentAsync(command.TicketId, cancellationToken)
            ?? throw new NotFoundException("Ticket", command.TicketId);

        EnsureTicketCanBeModified(ticket.Status);

        if (ticket.Priority == requestedPriority)
            throw new BusinessRuleException("Ticket already has this priority.");

        var previousPriority = ticket.Priority;
        ticket.Priority = requestedPriority;
        ticket.UpdatedAt = DateTime.UtcNow;

        await auditWriter.WriteAsync(
            "TicketPriorityChanged",
            "Ticket",
            ticket.Id,
            TicketAuditJson.Serialize(new
            {
                priority = previousPriority.ToString()
            }),
            TicketAuditJson.Serialize(new
            {
                priority = requestedPriority.ToString()
            }),
            cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return new UpdateTicketPriorityResponse(
            ticket.Id,
            ticket.TicketNumber,
            previousPriority.ToString(),
            requestedPriority.ToString(),
            "Ticket priority updated successfully.");
    }

    private static (TicketPriority Priority, string? ChangeReason) Validate(UpdateTicketPriorityCommand command)
    {
        var failures = new List<ValidationFailure>();
        if (command.TicketId == Guid.Empty) failures.Add(new ValidationFailure("ticketId", "Ticket is required."));

        TicketPriority parsedPriority = default;
        if (string.IsNullOrWhiteSpace(command.Priority))
            failures.Add(new ValidationFailure("priority", "Priority is required."));
        else if (!Enum.TryParse(command.Priority.Trim(), ignoreCase: true, out parsedPriority))
            failures.Add(new ValidationFailure("priority", $"Priority must be one of: {string.Join(", ", Enum.GetNames<TicketPriority>())}."));

        var changeReason = string.IsNullOrWhiteSpace(command.ChangeReason)
            ? null
            : command.ChangeReason.Trim();
        if (changeReason?.Length > ChangeReasonMaxLength)
            failures.Add(new ValidationFailure("changeReason", $"Change reason must be {ChangeReasonMaxLength} characters or fewer."));

        if (failures.Count > 0) throw new ValidationException(failures);
        return (parsedPriority, changeReason);
    }

    private static void EnsureTicketCanBeModified(TicketStatus status)
    {
        try
        {
            TicketWorkflowRules.EnsureCanModify(status);
        }
        catch (TicketDomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }
}

internal static class TicketValidation
{
    public static (string Category, string Subject, string Description, TicketPriority Priority) Validate(
        Guid customerId, Guid accountId, Guid campaignId,
        string? category, string? subject, string? description, string? priority)
    {
        var failures = new List<ValidationFailure>();

        if (customerId == Guid.Empty) failures.Add(new ValidationFailure("customerId", "Customer is required."));
        if (accountId == Guid.Empty) failures.Add(new ValidationFailure("accountId", "Account is required."));
        if (campaignId == Guid.Empty) failures.Add(new ValidationFailure("campaignId", "Campaign is required."));

        if (string.IsNullOrWhiteSpace(category)) failures.Add(new ValidationFailure("category", "Category is required."));
        else if (category.Trim().Length > 100) failures.Add(new ValidationFailure("category", "Category must be 100 characters or fewer."));

        if (string.IsNullOrWhiteSpace(subject)) failures.Add(new ValidationFailure("subject", "Subject is required."));
        else if (subject.Trim().Length > 200) failures.Add(new ValidationFailure("subject", "Subject must be 200 characters or fewer."));

        if (string.IsNullOrWhiteSpace(description)) failures.Add(new ValidationFailure("description", "Description is required."));

        TicketPriority parsedPriority = default;
        if (string.IsNullOrWhiteSpace(priority))
            failures.Add(new ValidationFailure("priority", "Priority is required."));
        else if (!Enum.TryParse<TicketPriority>(priority, ignoreCase: true, out parsedPriority))
            failures.Add(new ValidationFailure("priority", $"Priority must be one of: {string.Join(", ", Enum.GetNames<TicketPriority>())}."));

        if (failures.Count > 0) throw new ValidationException(failures);

        return (category!.Trim(), subject!.Trim(), description!.Trim(), parsedPriority);
    }
}

internal static class TicketSlaConstants
{
    public static TimeSpan GetSlaDuration(TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => TimeSpan.FromHours(4),
        TicketPriority.High => TimeSpan.FromHours(8),
        TicketPriority.Normal => TimeSpan.FromHours(24),
        TicketPriority.Low => TimeSpan.FromHours(48),
        _ => TimeSpan.FromHours(24)
    };
}

internal static class TicketAuditJson
{
    public static string Serialize(object value) => JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
}
