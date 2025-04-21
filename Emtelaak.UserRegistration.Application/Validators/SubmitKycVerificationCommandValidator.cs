// Emtelaak.UserRegistration.Application/Validators/SubmitKycVerificationCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class SubmitKycVerificationCommandValidator : AbstractValidator<SubmitKycVerificationCommand>
    {
        public SubmitKycVerificationCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.SubmissionData.PersonalInfo)
                .NotNull().WithMessage("Personal information is required.");

            RuleFor(x => x.SubmissionData.PersonalInfo.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .When(x => x.SubmissionData.PersonalInfo != null);

            RuleFor(x => x.SubmissionData.PersonalInfo.Nationality)
                .NotEmpty().WithMessage("Nationality is required.")
                .When(x => x.SubmissionData.PersonalInfo != null);

            RuleFor(x => x.SubmissionData.PersonalInfo.DateOfBirth)
                .NotNull().WithMessage("Date of birth is required.")
                .Must(BeAValidDate).WithMessage("Date of birth must be a valid date.")
                .LessThan(DateTime.UtcNow.AddYears(-18)).WithMessage("You must be at least 18 years old.")
                .When(x => x.SubmissionData.PersonalInfo != null);

            RuleFor(x => x.SubmissionData.Address)
                .NotNull().WithMessage("Address information is required.");

            RuleFor(x => x.SubmissionData.Address.Street)
                .NotEmpty().WithMessage("Street address is required.")
                .When(x => x.SubmissionData.Address != null);

            RuleFor(x => x.SubmissionData.Address.City)
                .NotEmpty().WithMessage("City is required.")
                .When(x => x.SubmissionData.Address != null);

            RuleFor(x => x.SubmissionData.Address.Country)
                .NotEmpty().WithMessage("Country is required.")
                .When(x => x.SubmissionData.Address != null);

            RuleFor(x => x.SubmissionData.DocumentIds)
                .NotNull().WithMessage("Document IDs are required.")
                .Must(ids => ids != null && ids.Count > 0).WithMessage("At least one document is required.");
        }

        private bool BeAValidDate(DateTime? date)
        {
            if (!date.HasValue)
                return false;

            return date.Value > DateTime.MinValue && date.Value < DateTime.MaxValue;
        }
    }
}