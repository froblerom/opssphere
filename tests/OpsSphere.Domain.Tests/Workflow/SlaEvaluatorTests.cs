using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;
using OpsSphere.Domain.Services;

namespace OpsSphere.Domain.Tests.Workflow;

public sealed class SlaEvaluatorTests
{
    private readonly SlaEvaluator evaluator = new();

    [Fact]
    public void Evaluate_WhenBeforeThreshold_ReturnsWithinSla()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
        var dueAt = startedAt.AddHours(10);

        var result = evaluator.Evaluate(startedAt, dueAt, 80, startedAt.AddHours(7));

        Assert.Equal(SlaState.WithinSla, result);
    }

    [Fact]
    public void Evaluate_WhenAtThreshold_ReturnsAtRisk()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
        var dueAt = startedAt.AddHours(10);

        var result = evaluator.Evaluate(startedAt, dueAt, 80, startedAt.AddHours(8));

        Assert.Equal(SlaState.AtRisk, result);
    }

    [Fact]
    public void Evaluate_WhenAfterThreshold_ReturnsAtRisk()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
        var dueAt = startedAt.AddHours(10);

        var result = evaluator.Evaluate(startedAt, dueAt, 80, startedAt.AddHours(9));

        Assert.Equal(SlaState.AtRisk, result);
    }

    [Fact]
    public void Evaluate_WhenPastDue_ReturnsBreached()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
        var dueAt = startedAt.AddHours(4);

        var result = evaluator.Evaluate(startedAt, dueAt, 80, dueAt.AddTicks(1));

        Assert.Equal(SlaState.Breached, result);
    }

    [Fact]
    public void Evaluate_WhenCompleted_ReturnsCompleted()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
        var slaState = new TicketSlaState
        {
            StartedAt = startedAt,
            DueAt = startedAt.AddHours(4),
            AtRiskThresholdPercent = 80,
            CompletedAt = startedAt.AddHours(2)
        };

        var result = evaluator.Evaluate(slaState, startedAt.AddDays(1));

        Assert.Equal(SlaState.Completed, result);
    }

    [Fact]
    public void Evaluate_WhenFinalStateExists_ReturnsCompleted()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);
        var slaState = new TicketSlaState
        {
            StartedAt = startedAt,
            DueAt = startedAt.AddHours(4),
            AtRiskThresholdPercent = 80,
            State = SlaState.Breached.ToString(),
            FinalState = SlaState.Breached.ToString()
        };

        var result = evaluator.Evaluate(slaState, startedAt.AddDays(1));

        Assert.Equal(SlaState.Completed, result);
    }

    [Fact]
    public void Evaluate_WhenDurationIsInvalid_ReturnsBreached()
    {
        var startedAt = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);

        var result = evaluator.Evaluate(startedAt, startedAt, 80, startedAt);

        Assert.Equal(SlaState.Breached, result);
    }
}
