// Emtelaak.UserRegistration.Application/Validators/ResendVerificationSmsCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class ResendVerificationSmsCommandValidator : AbstractValidator<ResendVerificationSmsCommand>
    {
        public ResendVerificationSmsCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be in a valid international format.");
        }
    }
}