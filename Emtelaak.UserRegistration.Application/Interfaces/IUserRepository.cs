// Emtelaak.UserRegistration.Application/Interfaces/IUserRepository.cs
using Emtelaak.UserRegistration.Domain.Entities;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdWithDetailsAsync(Guid userId);
        Task<List<User>> GetUsersByUserTypeAsync(Domain.Enums.UserType userType);

        // KYC related
        Task<KycVerification> AddKycVerificationAsync(KycVerification kycVerification);
        Task<KycVerification> GetKycVerificationByUserIdAsync(Guid userId);
        Task UpdateKycVerificationAsync(KycVerification kycVerification);

        // Document related
        Task<Document> AddDocumentAsync(Document document);
        Task<List<Document>> GetDocumentsByUserIdAsync(Guid userId);
        Task<Document> GetDocumentByIdAsync(Guid documentId);
        Task UpdateDocumentAsync(Document document);
        Task DeleteDocumentAsync(Guid documentId);

        // User preferences
        Task<UserPreference> AddUserPreferenceAsync(UserPreference userPreference);
        Task<UserPreference> GetUserPreferenceByUserIdAsync(Guid userId);
        Task UpdateUserPreferenceAsync(UserPreference userPreference);

        // Activity log
        Task<ActivityLog> AddActivityLogAsync(ActivityLog activityLog);
        Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(Guid userId, int limit = 20);

        // User sessions
        Task<UserSession> AddUserSessionAsync(UserSession userSession);
        Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId);
        Task UpdateUserSessionAsync(UserSession userSession);
        Task RevokeUserSessionAsync(Guid sessionId, string reason);
        Task RevokeAllUserSessionsExceptAsync(Guid userId, Guid activeSessionId, string reason);

        // Accreditation
        Task<Accreditation> AddAccreditationAsync(Accreditation accreditation);
        Task<Accreditation> GetAccreditationByUserIdAsync(Guid userId);
        Task<Accreditation> GetAccreditationByIdAsync(Guid accreditationId);
        Task UpdateAccreditationAsync(Accreditation accreditation);

        Task<UserSession> GetSessionByIdAsync(Guid sessionId);
        Task<UserSession> GetSessionByTokenAsync(string token);
    }
}
