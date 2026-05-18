using OpsSphere.Domain.Entities;
using OpsSphere.Domain.Enums;

namespace OpsSphere.Domain.Services;

public sealed class SlaEvaluator
{
    public SlaState Evaluate(TicketSlaState slaState, DateTime evaluatedAt)
    {
        if (slaState.CompletedAt.HasValue ||
            !string.IsNullOrWhiteSpace(slaState.FinalState) ||
            string.Equals(slaState.State, SlaState.Completed.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return SlaState.Completed;
        }

        return Evaluate(
            slaState.StartedAt,
            slaState.DueAt,
            slaState.AtRiskThresholdPercent,
            evaluatedAt);
    }

    public SlaState Evaluate(DateTime startedAt, DateTime dueAt, int atRiskThresholdPercent, DateTime evaluatedAt)
    {
        if (evaluatedAt >= dueAt)
            return SlaState.Breached;

        var totalTicks = dueAt.Ticks - startedAt.Ticks;
        if (totalTicks <= 0)
            return SlaState.Breached;

        var elapsedTicks = evaluatedAt.Ticks - startedAt.Ticks;
        if (elapsedTicks <= 0)
            return SlaState.WithinSla;

        var threshold = Math.Clamp(atRiskThresholdPercent, 1, 100);
        var thresholdTicks = totalTicks * threshold / 100;

        return elapsedTicks >= thresholdTicks
            ? SlaState.AtRisk
            : SlaState.WithinSla;
    }
}
