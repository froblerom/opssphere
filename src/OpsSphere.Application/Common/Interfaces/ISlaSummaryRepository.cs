using OpsSphere.Application.Features.SlaManagement;

namespace OpsSphere.Application.Common.Interfaces;

public interface ISlaSummaryRepository
{
    Task<SlaSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
