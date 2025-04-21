// Emtelaak.UserRegistration.Application/Validators/UpdateUserProfileCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.ProfileData.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.ProfileData.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.ProfileData.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be in a valid international format.");

            RuleFor(x => x.ProfileData.CountryOfResidence)
                .NotEmpty().WithMessage("Country of residence is required.");

            RuleFor(x => x.ProfileData.DateOfBirth)
                .Must(BeAValidDate).WithMessage("Date of birth must be a valid date.")
                .LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("You must be at least 18 years old.");
        }

        private bool BeAValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return true; // Optional field

            return date.Value > DateTime.MinValue && date.Value < DateTime.MaxValue;
        }
    }
}