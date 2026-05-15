using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Infrastructure.TicketManagement;

internal sealed class SequentialTicketNumberGenerator(ITicketRepository repository) : ITicketNumberGenerator
{
    public async Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");
        var latestSequence = await repository.GetLatestSequenceForDatePrefixAsync(datePrefix, cancellationToken);
        var nextSequence = latestSequence + 1;
        return $"OPS-{datePrefix}-{nextSequence:D6}";
    }
}
