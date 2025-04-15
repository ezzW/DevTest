// Emtelaak.UserRegistration.Application/DTOs/UserProfileUpdateDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserProfileUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string CountryOfResidence { get; set; }
    }
}
