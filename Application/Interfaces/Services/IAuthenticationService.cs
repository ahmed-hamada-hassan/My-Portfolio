using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IAuthenticationService : IScopedMark
{
    Task SignInAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task SignOutAsync(
        CancellationToken cancellationToken = default);
}