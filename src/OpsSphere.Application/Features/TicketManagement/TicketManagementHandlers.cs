using System.Text.Json;
using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;
using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;

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
