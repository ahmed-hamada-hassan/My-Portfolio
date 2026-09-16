using Domain.Errors;
using Domain.Shared;
using System.Net.Mail;

namespace Domain.Entities;

public class ContactMessage
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool IsRead { get; private set; }

    protected ContactMessage() { }

    private ContactMessage(string firstName, string lastName, string email, string subject, string message)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Subject = subject;
        Message = message;
        CreatedAt = DateTimeOffset.UtcNow;
        IsRead = false;
    }

    public static Result<ContactMessage> Create(string firstName, string lastName, string email, string subject, string message)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<ContactMessage>.Failure(ContactMessageErrors.InvalidFirstName);

        if (string.IsNullOrWhiteSpace(lastName))
            return Result<ContactMessage>.Failure(ContactMessageErrors.InvalidLastName);

        if (string.IsNullOrWhiteSpace(email) || !IsValidEmail(email))
            return Result<ContactMessage>.Failure(ContactMessageErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(subject))
            return Result<ContactMessage>.Failure(ContactMessageErrors.InvalidSubject);

        if (string.IsNullOrWhiteSpace(message))
            return Result<ContactMessage>.Failure(ContactMessageErrors.InvalidMessage);

        var contactMessage = new ContactMessage(firstName, lastName, email, subject, message);

        return Result<ContactMessage>.Success(contactMessage);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
