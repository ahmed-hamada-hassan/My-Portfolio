using Domain.Enums;
using Domain.Errors;
using Domain.Shared;

namespace Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string HashedPassword { get; private set; } = null!;
    public UserRole Role { get; private set; }

    protected User() { }

    private User(string email, string hashedPassword, UserRole role)
    {
        Id = Guid.NewGuid();
        Email = email;
        HashedPassword = hashedPassword;
        Role = role;
    }

    public static Result<User> Create(string email, string hashedPassword, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<User>.Failure(UserErrors.EmailCannotBeEmpty);

        if (string.IsNullOrWhiteSpace(hashedPassword))
            return Result<User>.Failure(UserErrors.PasswordCannotBeEmpty);

        var user = new User(email, hashedPassword, role);
        return Result<User>.Success(user);
    }
}
