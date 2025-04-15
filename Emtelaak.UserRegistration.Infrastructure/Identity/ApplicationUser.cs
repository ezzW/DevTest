using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
namespace Emtelaak.UserRegistration.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? CountryOfResidence { get; set; }
        public bool PhoneVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? TermsAcceptedVersion { get; set; }
        public DateTime? TermsAcceptedAt { get; set; }
        public string? ReferralCode { get; set; }
        public Guid? DomainUserId { get; set; } // Link to our domain User entity
    }
}
