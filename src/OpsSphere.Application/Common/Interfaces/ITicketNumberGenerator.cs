namespace OpsSphere.Application.Common.Interfaces;

public interface ITicketNumberGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}
