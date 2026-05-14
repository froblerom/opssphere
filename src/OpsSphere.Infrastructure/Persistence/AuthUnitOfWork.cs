using OpsSphere.Application.Common.Interfaces;

namespace OpsSphere.Infrastructure.Persistence;

internal sealed class AuthUnitOfWork : IAuthUnitOfWork
{
    private readonly OpsSphereDbContext dbContext;

    public AuthUnitOfWork(OpsSphereDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
