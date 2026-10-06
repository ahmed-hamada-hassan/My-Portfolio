using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserCommandRepository : IScopedMark
{
    Task<User?> GetByEmail(string email, CancellationToken cancellationToken = default);
}
