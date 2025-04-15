// Emtelaak.UserRegistration.Domain/Entities/User.cs
using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneVerified { get; set; }
        public bool EmailVerified { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CountryOfResidence { get; set; }
        public UserType UserType { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockoutEndDate { get; set; }
        public string TermsAcceptedVersion { get; set; }
        public DateTime? TermsAcceptedAt { get; set; }
        public string? ReferralCode { get; set; }

        // Navigation properties
        public KycVerification KycVerification { get; set; }
        public ICollection<Document> Documents { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public UserPreference UserPreference { get; set; }
        public Accreditation Accreditation { get; set; }
        public ICollection<ActivityLog> ActivityLogs { get; set; }

        public User()
        {
            Documents = new List<Document>();
            UserRoles = new List<UserRole>();
            ActivityLogs = new List<ActivityLog>();
        }
    }
}

