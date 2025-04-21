// Emtelaak.UserRegistration.Application/Validators/RegisterUserCommandValidator.cs
using FluentValidation;
using Emtelaak.UserRegistration.Application.Commands;
using System.Text.RegularExpressions;
using Emtelaak.UserRegistration.Application.Interfaces;

namespace Emtelaak.UserRegistration.Application.Validators
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        private readonly IUserRepository _userRepository;
        public RegisterUserCommandValidator(IUserRepository userRepository)
        {
            _userRepository = userRepository;

            RuleFor(x => x.RegistrationData.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                                .MustAsync(async (email, cancellation) =>
                                {
                                    var existingUser = await _userRepository.GetUserByEmailAsync(email);
                                    return existingUser == null;
                                }).WithMessage("This email address is already registered.");

            RuleFor(x => x.RegistrationData.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.RegistrationData.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.RegistrationData.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.RegistrationData.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be in a valid international format.");

            RuleFor(x => x.RegistrationData.CountryOfResidence)
                .NotEmpty().WithMessage("Country of residence is required.");

            RuleFor(x => x.RegistrationData.UserType)
                .IsInEnum().WithMessage("Invalid user type selected.");

            RuleFor(x => x.RegistrationData.TermsAccepted)
                .Equal(true).WithMessage("You must accept the terms and conditions.");

            RuleFor(x => x.RegistrationData.PrivacyAccepted)
                .Equal(true).WithMessage("You must accept the privacy policy.");
            _userRepository = userRepository;
        }
    }
}