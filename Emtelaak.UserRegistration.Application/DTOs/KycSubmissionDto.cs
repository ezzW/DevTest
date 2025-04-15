// Emtelaak.UserRegistration.Application/DTOs/KycSubmissionDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class KycSubmissionDto
    {
        public KycPersonalInfoDto PersonalInfo { get; set; }
        public KycAddressDto Address { get; set; }
        public List<Guid> DocumentIds { get; set; }
    }
}
