// Emtelaak.UserRegistration.Application/Validators/VerifyTwoFactorSetupCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class VerifyTwoFactorSetupCommandValidator : AbstractValidator<VerifyTwoFactorSetupCommand>
    {
        private readonly string[] _validMethods = { "SMS", "Authenticator", "Email" };

        public VerifyTwoFactorSetupCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Method)
                .NotEmpty().WithMessage("Two-factor method is required.")
                .Must(method => _validMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
                .WithMessage("Invalid two-factor method. Must be 'SMS', 'Authenticator', or 'Email'.");

            RuleFor(x => x.VerificationCode)
                .NotEmpty().WithMessage("Verification code is required.")
                .Matches("^[0-9]+$").WithMessage("Verification code must contain only digits.");
        }
    }
}