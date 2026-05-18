namespace OpsSphere.Application.Features.SlaManagement;

public sealed record SlaSummaryDto(
    int WithinSlaCount,
    int AtRiskCount,
    int BreachedCount,
    int CompletedCount);
