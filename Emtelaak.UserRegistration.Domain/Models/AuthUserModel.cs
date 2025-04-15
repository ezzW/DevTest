using System;
using System.Collections.Generic;

namespace Emtelaak.UserRegistration.Domain.Models
{
    public class AuthUserModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CountryOfResidence { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string TermsAcceptedVersion { get; set; }
        public DateTime? TermsAcceptedAt { get; set; }
        public string ReferralCode { get; set; }
        public Guid? DomainUserId { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}