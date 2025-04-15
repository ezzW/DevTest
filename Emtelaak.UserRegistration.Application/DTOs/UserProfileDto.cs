// Emtelaak.UserRegistration.Application/DTOs/UserProfileDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserProfileDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CountryOfResidence { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string UserType { get; set; }
        public string KycStatus { get; set; }
        public string AccreditationStatus { get; set; }
        public int ProfileCompletionPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
