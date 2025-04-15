// Emtelaak.UserRegistration.Application/Commands/RefreshTokenCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResultDto>
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RefreshTokenCommandHandler> _logger;

        public RefreshTokenCommandHandler(
            ITokenService tokenService,
            IUserRepository userRepository,
            ILogger<RefreshTokenCommandHandler> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LoginResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Processing token refresh");

                // Find user session
                var session = await _userRepository.GetSessionByTokenAsync(request.RefreshToken);
                if (session == null || !session.IsActive)
                {
                    _logger.LogWarning("Token refresh failed: Invalid or inactive refresh token");
                    return null;
                }

                // Get user ID from session
                var userId = session.UserId;

                // Refresh token
                var tokenResult = await _tokenService.RefreshTokenAsync(request.RefreshToken);

                // Return result
                return new LoginResultDto
                {
                    AccessToken = tokenResult.AccessToken,
                    RefreshToken = tokenResult.RefreshToken,
                    ExpiresIn = tokenResult.ExpiresIn,
                    UserId = userId,
                    RequiresMfa = false
                };
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Token refresh failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh: {Message}", ex.Message);
                throw;
            }
        }
    }
}