namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class MfaVerificationDto
    {
        public string MfaToken { get; set; }
        public string VerificationCode { get; set; }
        public string Method { get; set; }
    }
}
