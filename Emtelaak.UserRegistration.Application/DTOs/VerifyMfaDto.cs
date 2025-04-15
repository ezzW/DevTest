// Emtelaak.UserRegistration.Application/DTOs/VerifyMfaDto.cs
// Emtelaak.UserRegistration.Application/DTOs/VerifyMfaDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class VerifyMfaDto
    {
        public string MfaToken { get; set; }
        public string VerificationCode { get; set; }
        public string Method { get; set; }
    }
}