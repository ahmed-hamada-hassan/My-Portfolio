using Application.Interfaces.DI;
using Domain.Entities;

namespace Application.Interfaces.Services;

public interface IPasswordHasher : IScopedMark
{
    string Hash(User user, string password);

    bool Verify(
        User user,
        string hashedPassword,
        string password);
}