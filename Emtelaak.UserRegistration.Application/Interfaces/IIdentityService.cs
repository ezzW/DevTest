// Emtelaak.UserRegistration.Application/Interfaces/IIdentityService.cs
using Emtelaak.UserRegistration.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<IdentityResultModel> CreateUserAsync(AuthUserModel user, string password);
        Task<AuthUserModel> FindUserByEmailAsync(string email);
        Task<AuthUserModel> FindUserByIdAsync(string userId);
        Task<bool> CheckPasswordAsync(AuthUserModel user, string password);
        Task<AuthResult> ValidateUserCredentialsAsync(string email, string password);
        Task<IdentityResultModel> ConfirmEmailAsync(AuthUserModel user, string token);
        Task<IdentityResultModel> VerifyEmailAsync(AuthUserModel user, string token);
        Task<string> GenerateEmailVerificationTokenAsync(AuthUserModel user);
        Task<IdentityResultModel> VerifyPhoneNumberAsync(AuthUserModel user, string token);
        Task<string> GeneratePhoneVerificationTokenAsync(AuthUserModel user);
        Task<IdentityResultModel> ChangePasswordAsync(AuthUserModel user, string currentPassword, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(AuthUserModel user);
        Task<IdentityResultModel> ResetPasswordAsync(AuthUserModel user, string token, string newPassword);
        Task<bool> IsEmailConfirmedAsync(AuthUserModel user);
        Task<bool> IsPhoneNumberConfirmedAsync(AuthUserModel user);
        Task<bool> IsTwoFactorEnabledAsync(AuthUserModel user);
        Task<IList<string>> GetValidTwoFactorProvidersAsync(AuthUserModel user);
        Task<bool> VerifyTwoFactorTokenAsync(AuthUserModel user, string provider, string token);
        Task<string> GenerateTwoFactorTokenAsync(AuthUserModel user, string provider);
        Task<IdentityResultModel> SetTwoFactorEnabledAsync(AuthUserModel user, bool enabled);
        Task<IdentityResultModel> SetPhoneNumberAsync(AuthUserModel user, string phoneNumber);
        Task<IdentityResultModel> AddToRoleAsync(AuthUserModel user, string role);
        Task<IdentityResultModel> RemoveFromRoleAsync(AuthUserModel user, string role);
        Task<IList<string>> GetRolesAsync(AuthUserModel user);
        Task<bool> IsInRoleAsync(AuthUserModel user, string role);
        Task<IdentityResultModel> LockoutUserAsync(AuthUserModel user, DateTimeOffset endDate);
        Task<IdentityResultModel> RemoveLockoutAsync(AuthUserModel user);
        Task<AuthUserModel> FindByDomainUserIdAsync(Guid domainUserId);
        Task<IdentityResultModel> UpdateLastLoginDateAsync(AuthUserModel user);
        Task<IdentityResultModel> UpdateUserAsync(AuthUserModel user);
        Task<IdentityResultModel> VerifyEmailWithCodeAsync(AuthUserModel user, string code);
        Task<string> GenerateEmailVerificationCodeAsync(AuthUserModel user);
        Task<string> GeneratePasswordResetCodeAsync(AuthUserModel user);
        Task<IdentityResultModel> ResetPasswordWithCodeAsync(AuthUserModel user, string code, string newPassword);

    }
}
