// Emtelaak.UserRegistration.Application/Interfaces/IKycVerificationService.cs
using Emtelaak.UserRegistration.Domain.Entities;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IKycVerificationService
    {
        Task<KycVerification> SubmitVerificationAsync(Guid userId, KycVerification verification);
        Task<KycVerification> UpdateVerificationStatusAsync(Guid verificationId, Domain.Enums.KycStatus status, string remarks = null);
        Task<bool> VerifyDocumentAsync(Guid documentId, bool isVerified, string rejectionReason = null);
        Task<string> GetVerificationProviderUrlAsync(Guid userId);
        Task<bool> ProcessVerificationWebhookAsync(string payload);
    }
}
