// Emtelaak.UserRegistration.API/Controllers/AuthController.cs
using System;
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

            var command = new RegisterUserCommand { RegistrationData = registrationDto };
            var result = await _mediator.Send(command);

            return Created($"/api/users/{result.UserId}", result);

        }

        //[HttpPost("verify-email")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<EmailVerificationResultDto>> VerifyEmail([FromBody] EmailVerificationDto verificationDto)
        //{
        //    try
        //    {
        //        var command = new VerifyEmailCommand { VerificationData = verificationDto };
        //        var result = await _mediator.Send(command);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error during email verification");
        //        return BadRequest(new { errors = new { general = new[] { "Email verification failed. Please try again." } } });
        //    }
        //}

        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmailVerificationResultDto>> VerifyEmail([FromBody] EmailVerificationDto verificationDto)
        {

            var command = new VerifyEmailCommand { VerificationData = verificationDto };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("verify-phone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PhoneVerificationResultDto>> VerifyPhone([FromBody] PhoneVerificationDto verificationDto)
        {
            var command = new VerifyPhoneCommand { VerificationData = verificationDto };
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginDto loginDto)
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

            return Ok(result);
        }

        [HttpPost("mfa-verify")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResultDto>> VerifyMfa([FromBody] MfaVerificationDto mfaDto)
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

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResultDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var command = new RefreshTokenCommand { RefreshToken = refreshTokenDto.RefreshToken };
            var result = await _mediator.Send(command);

            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutDto logoutDto)
        {
            var command = new LogoutCommand { RefreshToken = logoutDto.RefreshToken };
            await _mediator.Send(command);

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("resend-verification-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto resendDto)
        {
            var command = new ResendVerificationEmailCommand { Email = resendDto.Email };
            await _mediator.Send(command);

            return Ok(new { message = "Verification email sent successfully" });
        }

        [HttpPost("resend-verification-sms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendVerificationSms([FromBody] ResendVerificationSmsDto resendDto)
        {
            var command = new ResendVerificationSmsCommand { PhoneNumber = resendDto.PhoneNumber };
            await _mediator.Send(command);

            return Ok(new { message = "Verification SMS sent successfully" });
        }


        /// <summary>
        /// Get all countries or search countries by name
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter countries</param>
        /// <returns>List of countries</returns>
        [HttpGet("countries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CountryDto>>> GetCountries([FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = new GetCountriesQuery { SearchTerm = searchTerm };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving countries" });
            }
        }

        /// <summary>
        /// Get all country phone codes or search by country name/code
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter countries</param>
        /// <returns>List of country phone codes</returns>
        [HttpGet("countries/phone-codes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CountryPhoneCodeDto>>> GetCountryPhoneCodes([FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = new GetCountryPhoneCodesQuery { SearchTerm = searchTerm };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving country phone codes");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving country phone codes" });
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
                    Message = "If your email is registered, you will receive a reset code shortly",
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

        //[HttpPost("enable-two-factor")]
        //[Authorize]
        //public async Task<ActionResult<EnableTwoFactorResultDto>> EnableTwoFactor([FromBody] EnableTwoFactorDto request)
        //{
        //    try
        //    {
        //        var userId = GetUserIdFromClaims();
        //        var command = new EnableTwoFactorCommand
        //        {
        //            UserId = userId,
        //            Method = request.Method,
        //            Enable = request.Enable
        //        };

        //        var result = await _mediator.Send(command);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error enabling two-factor authentication");
        //        return BadRequest(new { message = "Failed to enable two-factor authentication" });
        //    }
        //}
    }
}