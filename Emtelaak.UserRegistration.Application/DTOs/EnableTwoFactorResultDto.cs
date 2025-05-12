// Emtelaak.UserRegistration.Application/DTOs/EnableTwoFactorResultDto.cs
public class EnableTwoFactorResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string SetupCode { get; set; }
    public string QrCodeUrl { get; set; }
}