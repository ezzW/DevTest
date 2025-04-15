// Emtelaak.UserRegistration.Application/DTOs/KycPersonalInfoDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class KycPersonalInfoDto
    {
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Gender { get; set; }
    }
}
