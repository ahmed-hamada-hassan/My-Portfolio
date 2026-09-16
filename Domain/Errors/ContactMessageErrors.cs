using Domain.Shared;

namespace Domain.Errors;

public class ContactMessageErrors
{
    public static readonly Error InvalidFirstName = new Error("InvalidFirstName", "First name cannot be empty.");
    public static readonly Error InvalidLastName = new Error("InvalidLastName", "Last name cannot be empty.");
    public static readonly Error InvalidEmail = new Error("InvalidEmail", "Email must be a valid email address.");
    public static readonly Error InvalidSubject = new Error("InvalidSubject", "Subject cannot be empty.");
    public static readonly Error InvalidMessage = new Error("InvalidMessage", "Message cannot be empty.");
}
