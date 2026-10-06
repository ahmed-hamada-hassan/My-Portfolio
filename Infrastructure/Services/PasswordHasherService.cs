using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

internal sealed class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();
    public string Hash(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool Verify(User user, string hashedPassword, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);

        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}
