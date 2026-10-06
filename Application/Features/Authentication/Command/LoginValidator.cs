using Domain.Constants;
using FluentValidation;

namespace Application.Features.Authentication.Command;

internal sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.loginRequest.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(MaxLength.UserEmail)
            .WithMessage(
                $"Email must not exceed {MaxLength.UserEmail} characters.");

        RuleFor(x => x.loginRequest.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
