// Emtelaak.UserRegistration.Application/DTOs/UserRegistrationDto.cs
using System;
using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserRegistrationDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryOfResidence { get; set; }
        public UserType UserType { get; set; }
        public bool TermsAccepted { get; set; }
        public bool PrivacyAccepted { get; set; }
    }
}