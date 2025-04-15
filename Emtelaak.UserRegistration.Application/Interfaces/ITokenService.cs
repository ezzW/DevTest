// Emtelaak.UserRegistration.Application/Interfaces/ITokenService.cs
using Emtelaak.UserRegistration.Domain.Models;

namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResult> GenerateAuthTokensAsync(AuthUserModel user);
        Task<string> GenerateMfaTokenAsync(AuthUserModel user);
        Task<(bool isValid, string email)> ValidateMfaTokenAsync(string token);
        Task<TokenResult> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
    }
}