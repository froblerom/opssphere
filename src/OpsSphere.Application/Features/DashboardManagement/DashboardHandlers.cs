using OpsSphere.Application.Common.Exceptions;
using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.DashboardManagement;

public sealed class GetOperationalDashboardQueryHandler(IDashboardRepository repository)
{
    public Task<OperationalDashboardDto> HandleAsync(GetOperationalDashboardQuery query, CancellationToken cancellationToken)
    {
        if (query.DateFrom.HasValue && query.DateTo.HasValue && query.DateFrom.Value > query.DateTo.Value)
            throw new ValidationException("dateFrom", "Date from must be earlier than or equal to date to.");

        return repository.GetOperationalDashboardAsync(query, cancellationToken);
    }
}
