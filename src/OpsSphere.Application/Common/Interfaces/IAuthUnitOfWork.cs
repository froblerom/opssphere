namespace OpsSphere.Application.Common.Interfaces;

public interface IAuthUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
