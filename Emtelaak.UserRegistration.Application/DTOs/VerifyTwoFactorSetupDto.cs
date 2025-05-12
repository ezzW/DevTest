// Emtelaak.UserRegistration.Application/DTOs/VerifyTwoFactorSetupDto.cs
public class VerifyTwoFactorSetupDto
{
    public string Method { get; set; }
    public string VerificationCode { get; set; }
}