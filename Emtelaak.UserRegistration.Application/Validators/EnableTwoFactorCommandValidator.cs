// Emtelaak.UserRegistration.Application/Validators/EnableTwoFactorCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class EnableTwoFactorCommandValidator : AbstractValidator<EnableTwoFactorCommand>
    {
        private readonly string[] _validMethods = { "SMS", "Authenticator", "Email" };

        public EnableTwoFactorCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Method)
                .NotEmpty().WithMessage("Two-factor method is required.")
                .Must(method => _validMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Invalid two-factor method. Must be 'SMS', 'Authenticator', or 'Email'.");
        }
    }
}