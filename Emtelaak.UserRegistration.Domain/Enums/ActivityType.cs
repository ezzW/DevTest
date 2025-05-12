// Emtelaak.UserRegistration.Domain/Enums/ActivityType.cs
namespace Emtelaak.UserRegistration.Domain.Enums
{
    public enum ActivityType
    {
        Login = 1,
        Logout = 2,
        ProfileUpdate = 3,
        PasswordChange = 4,
        VerificationSubmit = 5,
        EmailVerification = 6,
        PhoneVerification = 7,
        AccountCreation = 8,
        KycSubmission = 9,
        DocumentUpload = 10,
        AccreditationUpdate = 11,
        TwoFactorEnabled = 12,
        TwoFactorDisabled = 13,
        PasswordReset = 14,
        AccreditationSubmitted = 15,
        AccreditationApproved = 16,
        AccreditationRejected = 17,
        AccreditationExpired = 18,
        AccreditationDocumentUpload = 19,
        AccreditationStatusUpdated = 20
    }
}