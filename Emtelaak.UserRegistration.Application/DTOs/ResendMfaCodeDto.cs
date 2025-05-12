// Emtelaak.UserRegistration.Application/DTOs/ResendMfaCodeDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class ResendMfaCodeDto
    {
        public string MfaToken { get; set; }
        public string Method { get; set; }
    }
}
