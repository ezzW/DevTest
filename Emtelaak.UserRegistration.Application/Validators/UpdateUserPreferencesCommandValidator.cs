// Emtelaak.UserRegistration.Application/Validators/UpdateUserPreferencesCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
    {
        private readonly string[] _validThemes = { "Light", "Dark", "System" };

        public UpdateUserPreferencesCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Preferences)
                .NotNull().WithMessage("Preferences data is required.");

            RuleFor(x => x.Preferences.Language)
                .NotEmpty().WithMessage("Language is required.");

            RuleFor(x => x.Preferences.Theme)
                .NotEmpty().WithMessage("Theme is required.")
                .Must(theme => _validThemes.Contains(theme))
                .WithMessage("Invalid theme. Must be 'Light', 'Dark', or 'System'.");
        }
    }
}