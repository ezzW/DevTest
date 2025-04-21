// Emtelaak.UserRegistration.Application/Validators/ChangePasswordCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.PasswordData.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.PasswordData.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.")
                .NotEqual(x => x.PasswordData.CurrentPassword).WithMessage("New password cannot be the same as current password.");

            RuleFor(x => x.PasswordData.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.PasswordData.NewPassword).WithMessage("Passwords do not match.");
        }
    }
}