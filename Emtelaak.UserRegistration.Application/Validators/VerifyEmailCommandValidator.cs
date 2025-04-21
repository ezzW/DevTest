// Emtelaak.UserRegistration.Application/Validators/VerifyEmailCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(x => x.VerificationData.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.VerificationData.VerificationCode)
                .NotEmpty().WithMessage("Verification code is required.");
        }
    }
}