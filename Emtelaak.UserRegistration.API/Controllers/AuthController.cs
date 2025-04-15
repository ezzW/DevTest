// Emtelaak.UserRegistration.API/Controllers/AuthController.cs
using System;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IMediator mediator,
            ILogger<AuthController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserRegistrationResultDto>> Register([FromBody] UserRegistrationDto registrationDto)
        {
            try
            {
                var command = new RegisterUserCommand { RegistrationData = registrationDto };
                var result = await _mediator.Send(command);

                return Created($"/api/users/{result.UserId}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return BadRequest(new { errors = new { general = new[] { "Registration failed. Please try again." } } });
            }
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmailVerificationResultDto>> VerifyEmail([FromBody] EmailVerificationDto verificationDto)
        {
            try
            {
                var command = new VerifyEmailCommand { VerificationData = verificationDto };
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification");
                return BadRequest(new { errors = new { general = new[] { "Email verification failed. Please try again." } } });
            }
        }

        [HttpPost("verify-phone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PhoneVerificationResultDto>> VerifyPhone([FromBody] PhoneVerificationDto verificationDto)
        {
            try
            {
                var command = new VerifyPhoneCommand { VerificationData = verificationDto };
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during phone verification");
                return BadRequest(new { errors = new { general = new[] { "Phone verification failed. Please try again." } } });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var command = new LoginUserCommand
                {
                    LoginData = loginDto,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                var result = await _mediator.Send(command);

                if (result == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                if (result.RequiresMfa)
                {
                    return Ok(result); // Return MFA info
                }

                if (string.IsNullOrEmpty(result.AccessToken))
                {
                    return Unauthorized(new { message = "Authentication failed" });
                }

                // Add token type to the response
                var enhancedResult = new
                {
                    access_token = result.AccessToken,
                    refresh_token = result.RefreshToken,
                    expires_in = result.ExpiresIn,
                    token_type = "Bearer",
                    user_id = result.UserId
                };

                return Ok(enhancedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return Unauthorized(new { message = "Login failed" });
            }
        }

        [HttpPost("mfa-verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResultDto>> VerifyMfa([FromBody] MfaVerificationDto mfaDto)
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var command = new VerifyMfaCommand
                {
                    VerificationData = mfaDto,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                var result = await _mediator.Send(command);

                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return Unauthorized(new { message = "Invalid verification code" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during MFA verification");
                return Unauthorized(new { message = "MFA verification failed" });
            }
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ForgotPasswordResultDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var command = new ForgotPasswordCommand { Email = forgotPasswordDto.Email };
                var result = await _mediator.Send(command);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request");
                // Always return success to prevent email enumeration attacks
                return Ok(new ForgotPasswordResultDto
                {
                    Message = "If your email is registered, you will receive a reset link shortly",
                    EmailSent = false
                });
            }
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResetPasswordResultDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var command = new ResetPasswordCommand { ResetData = resetPasswordDto };
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return BadRequest(new ResetPasswordResultDto
                {
                    Success = false,
                    Message = "Password reset failed. Please try again."
                });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResultDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var command = new RefreshTokenCommand { RefreshToken = refreshTokenDto.RefreshToken };
                var result = await _mediator.Send(command);

                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                {
                    return Unauthorized(new { message = "Invalid refresh token" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return Unauthorized(new { message = "Token refresh failed" });
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            try
            {
                var command = new LogoutCommand { RefreshToken = logoutDto.RefreshToken };
                await _mediator.Send(command);

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Always return success to avoid information leakage
                return Ok(new { message = "Logged out successfully" });
            }
        }

        [HttpPost("resend-verification-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto resendDto)
        {
            try
            {
                var command = new ResendVerificationEmailCommand { Email = resendDto.Email };
                await _mediator.Send(command);

                return Ok(new { message = "Verification email sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification email");
                // Always return success to prevent email enumeration attacks
                return Ok(new { message = "If your email is registered, you will receive a verification email shortly" });
            }
        }

        [HttpPost("resend-verification-sms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendVerificationSms([FromBody] ResendVerificationSmsDto resendDto)
        {
            try
            {
                var command = new ResendVerificationSmsCommand { PhoneNumber = resendDto.PhoneNumber };
                await _mediator.Send(command);

                return Ok(new { message = "Verification SMS sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification SMS");
                // Always return success to prevent phone number enumeration attacks
                return Ok(new { message = "If your phone number is registered, you will receive a verification SMS shortly" });
            }
        }
    }
}