using Domain.Shared;

namespace Domain.Errors;

public class UserErrors
{
    public static readonly Error EmailCannotBeEmpty = new Error("User.InvalidEmail", "Email cannot be empty.");
    public static readonly Error PasswordCannotBeEmpty = new Error("User.InvalidPassword", "Password cannot be empty.");
    public static readonly Error InvalidCredentials = new Error("User.InvalidCredentials", "Invalid email or password.");
}
