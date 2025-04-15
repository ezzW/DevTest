// Emtelaak.UserRegistration.Application/DTOs/KycAddressDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class KycAddressDto
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }
}