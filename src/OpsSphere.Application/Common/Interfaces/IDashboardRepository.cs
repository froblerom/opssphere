using OpsSphere.Application.Features.DashboardManagement;

namespace OpsSphere.Application.Common.Interfaces;

public interface IDashboardRepository
{
    Task<OperationalDashboardDto> GetOperationalDashboardAsync(GetOperationalDashboardQuery query, CancellationToken cancellationToken);
}
