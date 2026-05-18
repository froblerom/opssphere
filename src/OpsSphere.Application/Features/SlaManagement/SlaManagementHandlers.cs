using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Application.Features.SlaManagement;

public sealed class GetSlaSummaryQueryHandler(ISlaSummaryRepository repository)
{
    public Task<SlaSummaryDto> HandleAsync(GetSlaSummaryQuery query, CancellationToken cancellationToken) =>
        repository.GetSummaryAsync(cancellationToken);
}
