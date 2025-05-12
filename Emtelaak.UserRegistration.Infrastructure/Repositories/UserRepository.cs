// Emtelaak.UserRegistration.Infrastructure/Repositories/UserRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using Emtelaak.UserRegistration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext dbContext, ILogger<UserRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<TEntity> GetByIdAsync<TEntity>(Guid id) where TEntity : class
        {
            return await _dbContext.Set<TEntity>().FindAsync(id);
        }

        public async Task<IReadOnlyList<User>> GetAllAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<IReadOnlyList<User>> GetAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbContext.Users.Where(predicate).ToListAsync();
        }

        public async Task<User> AddAsync(User entity)
        {
            await _dbContext.Users.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<User>> AddRangeAsync(IEnumerable<User> entities)
        {
            await _dbContext.Users.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task UpdateAsync(User entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByIdWithDetailsAsync(Guid userId)
        {
            return await _dbContext.Users
                .Include(u => u.KycVerification)
                .Include(u => u.Accreditation)
                .Include(u => u.UserPreference)
                .Include(u => u.Documents)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<User>> GetUsersByUserTypeAsync(UserType userType)
        {
            return await _dbContext.Users
                .Where(u => u.UserType == userType)
                .ToListAsync();
        }

        public async Task<KycVerification> AddKycVerificationAsync(KycVerification kycVerification)
        {
            // Check if user already has KYC verification
            var existingVerification = await _dbContext.KycVerifications
                .FirstOrDefaultAsync(k => k.UserId == kycVerification.UserId);

            if (existingVerification != null)
            {
                _logger.LogWarning("User {UserId} already has KYC verification", kycVerification.UserId);
                throw new InvalidOperationException($"User {kycVerification.UserId} already has KYC verification");
            }

            await _dbContext.KycVerifications.AddAsync(kycVerification);
            await _dbContext.SaveChangesAsync();
            return kycVerification;
        }

        public async Task<KycVerification> GetKycVerificationByUserIdAsync(Guid userId)
        {
            return await _dbContext.KycVerifications
                .FirstOrDefaultAsync(k => k.UserId == userId);
        }

        public async Task UpdateKycVerificationAsync(KycVerification kycVerification)
        {
            _dbContext.Entry(kycVerification).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Document> AddDocumentAsync(Document document)
        {
            await _dbContext.Documents.AddAsync(document);
            await _dbContext.SaveChangesAsync();
            return document;
        }

        public async Task<List<Document>> GetDocumentsByUserIdAsync(Guid userId)
        {
            return await _dbContext.Documents
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<Document> GetDocumentByIdAsync(Guid documentId)
        {
            return await _dbContext.Documents.FindAsync(documentId);
        }

        public async Task UpdateDocumentAsync(Document document)
        {
            _dbContext.Entry(document).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteDocumentAsync(Guid documentId)
        {
            var document = await _dbContext.Documents.FindAsync(documentId);
            if (document != null)
            {
                _dbContext.Documents.Remove(document);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<UserPreference> AddUserPreferenceAsync(UserPreference userPreference)
        {
            // Check if user already has preferences
            var existingPreference = await _dbContext.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userPreference.UserId);

            if (existingPreference != null)
            {
                _logger.LogWarning("User {UserId} already has preferences", userPreference.UserId);
                throw new InvalidOperationException($"User {userPreference.UserId} already has preferences");
            }

            await _dbContext.UserPreferences.AddAsync(userPreference);
            await _dbContext.SaveChangesAsync();
            return userPreference;
        }

        public async Task<UserPreference> GetUserPreferenceByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateUserPreferenceAsync(UserPreference userPreference)
        {
            _dbContext.Entry(userPreference).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ActivityLog> AddActivityLogAsync(ActivityLog activityLog)
        {
            await _dbContext.ActivityLogs.AddAsync(activityLog);
            await _dbContext.SaveChangesAsync();
            return activityLog;
        }

        public async Task<List<ActivityLog>> GetActivityLogsByUserIdAsync(Guid userId, int limit = 20)
        {
            return await _dbContext.ActivityLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<UserSession> AddUserSessionAsync(UserSession userSession)
        {
            await _dbContext.UserSessions.AddAsync(userSession);
            await _dbContext.SaveChangesAsync();
            return userSession;
        }

        public async Task<List<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId)
        {
            return await _dbContext.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();
        }

        public async Task UpdateUserSessionAsync(UserSession userSession)
        {
            _dbContext.Entry(userSession).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RevokeUserSessionAsync(Guid sessionId, string reason)
        {
            var session = await _dbContext.UserSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.IsActive = false;
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = reason;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RevokeAllUserSessionsExceptAsync(Guid userId, Guid activeSessionId, string reason)
        {
            var sessions = await _dbContext.UserSessions
                .Where(s => s.UserId == userId && s.Id != activeSessionId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedReason = reason;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<Accreditation> AddAccreditationAsync(Accreditation accreditation)
        {
            // Check if user already has accreditation
            var existingAccreditation = await _dbContext.Accreditations
                .FirstOrDefaultAsync(a => a.UserId == accreditation.UserId);

            if (existingAccreditation != null)
            {
                _logger.LogWarning("User {UserId} already has accreditation", accreditation.UserId);
                throw new InvalidOperationException($"User {accreditation.UserId} already has accreditation");
            }

            await _dbContext.Accreditations.AddAsync(accreditation);
            await _dbContext.SaveChangesAsync();
            return accreditation;
        }

        public async Task<Accreditation> GetAccreditationByUserIdAsync(Guid userId)
        {
            return await _dbContext.Accreditations
                .FirstOrDefaultAsync(a => a.UserId == userId);
        }
        
        public async Task<Accreditation> GetAccreditationByIdAsync(Guid accreditationId)
        {
            return await _dbContext.Accreditations
                .FirstOrDefaultAsync(a => a.Id == accreditationId);
        }

        public async Task UpdateAccreditationAsync(Accreditation accreditation)
        {
            _dbContext.Entry(accreditation).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserSession> GetSessionByIdAsync(Guid sessionId)
        {
            return await _dbContext.UserSessions.FindAsync(sessionId);
        }

        public async Task<UserSession> GetSessionByTokenAsync(string token)
        {
            return await _dbContext.UserSessions
                .FirstOrDefaultAsync(s => s.Token == token && s.IsActive);
        }
    }
}