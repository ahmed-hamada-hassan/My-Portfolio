using Domain.Shared;

namespace Domain.Errors;

public class UserErrors
{
    public static Error EmailCannotBeEmpty = new Error("User.InvalidEmail", "Email cannot be empty.");
    public static Error PasswordCannotBeEmpty = new Error("User.InvalidPassword", "Password cannot be empty.");
    public static Error InvalidCredentials = new Error("User.InvalidCredentials", "Invalid email or password.");

}
