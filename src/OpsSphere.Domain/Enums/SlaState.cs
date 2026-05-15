namespace OpsSphere.Domain.Enums;

public enum SlaState
{
    WithinSla = 1,
    AtRisk = 2,
    Breached = 3,
    Completed = 4
}
