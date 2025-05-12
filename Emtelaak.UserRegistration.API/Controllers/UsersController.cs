// Emtelaak.UserRegistration.API/Controllers/UsersController.cs
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IMediator mediator,
            ILogger<UsersController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            try
            {
                // Get the user ID from the claims
                var userId = GetUserIdFromClaims();

                var query = new GetUserProfileQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "User profile not found");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileUpdateResultDto>> UpdateProfile([FromBody] UserProfileUpdateDto profileUpdateDto)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var command = new UpdateUserProfileCommand
                {
                    UserId = userId,
                    ProfileData = profileUpdateDto
                };

                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "User profile update failed");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("preferences")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserPreferenceUpdateResultDto>> UpdatePreferences([FromBody] UserPreferenceDto preferencesDto)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var command = new UpdateUserPreferencesCommand
                {
                    UserId = userId,
                    Preferences = preferencesDto
                };

                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "User preferences update failed");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PasswordChangeResultDto>> ChangePassword([FromBody] PasswordChangeDto passwordChangeDto)
        {
                var userId = GetUserIdFromClaims();

                var command = new ChangePasswordCommand
                {
                    UserId = userId,
                    PasswordData = passwordChangeDto
                };

                var result = await _mediator.Send(command);

                if (!result.Updated)
                {
                    return BadRequest(result);
                }

                return Ok(result);       
        }

        [HttpPost("profile-picture")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentUploadResultDto>> UploadProfilePicture(IFormFile file)
        {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }

                // Check file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!Array.Exists(allowedTypes, type => type == file.ContentType))
                {
                    return BadRequest(new { message = "Invalid file type. Only JPEG, PNG and GIF are supported." });
                }

                // Check file size (limit to 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File size exceeds 5MB limit." });
                }

                var userId = GetUserIdFromClaims();

                var command = new UploadProfilePictureCommand
                {
                    UserId = userId,
                    File = file
                };

                var result = await _mediator.Send(command);

                return Ok(result);
        }

        [HttpGet("sessions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserSessionsDto>> GetSessions()
        {
                var userId = GetUserIdFromClaims();

                var query = new GetUserSessionsQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(result);
        }

        [HttpPost("sessions/{sessionId}/revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeSession(Guid sessionId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var command = new RevokeSessionCommand
                {
                    UserId = userId,
                    SessionId = sessionId,
                    Reason = "User initiated logout"
                };

                await _mediator.Send(command);

                return Ok(new { message = "Session revoked successfully" });
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Session revoke failed");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("sessions/revoke-all-except")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RevokeAllSessionsExcept([FromBody] RevokeAllSessionsExceptDto revokeDto)
        {
                var userId = GetUserIdFromClaims();

                var command = new RevokeAllSessionsExceptCommand
                {
                    UserId = userId,
                    ActiveSessionId = revokeDto.ActiveSessionId,
                    Reason = "User initiated logout from all devices"
                };

                await _mediator.Send(command);

                return Ok(new { message = "All other sessions revoked successfully" });
        }

        [HttpGet("activity-log")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ActivityLogListDto>> GetActivityLog([FromQuery] int limit = 20)
        {
                var userId = GetUserIdFromClaims();

                var query = new GetUserActivityLogQuery
                {
                    UserId = userId,
                    Limit = Math.Min(limit, 100) // Cap at 100 entries
                };

                var result = await _mediator.Send(query);

                return Ok(result);
        }

        [HttpPost("enable-two-factor")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EnableTwoFactorResultDto>> EnableTwoFactor([FromBody] EnableTwoFactorDto request)
        {
                var userId = GetUserIdFromClaims();
                var command = new EnableTwoFactorCommand
                {
                    UserId = userId,
                    Method = request.Method,
                    Enable = request.Enable
                };

                var result = await _mediator.Send(command);
                return Ok(result);
        }

        [HttpPost("verify-two-factor-setup")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> VerifyTwoFactorSetup([FromBody] VerifyTwoFactorSetupDto request)
        {
                var userId = GetUserIdFromClaims();
                var command = new VerifyTwoFactorSetupCommand
                {
                    UserId = userId,
                    Method = request.Method,
                    VerificationCode = request.VerificationCode
                };

                var result = await _mediator.Send(command);
                if (!result)
                {
                    return BadRequest(new { message = "Invalid verification code" });
                }

                return Ok(new { success = true, message = "Two-factor authentication has been enabled" });
        }

        [HttpGet("two-factor-status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TwoFactorStatusDto>> GetTwoFactorStatus()
        {
                var userId = GetUserIdFromClaims();
                var query = new GetTwoFactorStatusQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(result);
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ??
                      User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                      User.FindFirst("domainUserId")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new ApplicationException("Invalid user identifier in token");
            }

            return userId;
        }
    }
}