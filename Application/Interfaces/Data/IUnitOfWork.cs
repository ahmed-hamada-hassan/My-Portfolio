using Application.Interfaces.DI;

namespace Application.Interfaces.Data;

public interface IUnitOfWork : IScopedMark
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
