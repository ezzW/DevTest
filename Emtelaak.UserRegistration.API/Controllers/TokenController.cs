using Duende.IdentityModel.Client;
using Emtelaak.UserRegistration.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TokenController> _logger;
    private readonly IConfiguration _configuration;

    public TokenController(
        UserManager<ApplicationUser> userManager,
        ILogger<TokenController> logger,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> GetToken([FromBody] LoginRequest model)
    {
        try
        {
            // Verify user credentials first
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if email is confirmed if required
            if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
            {
                _logger.LogWarning("Login attempt for unconfirmed email: {Email}", model.Email);
                return BadRequest(new { message = "Please confirm your email before logging in" });
            }

            // Check if user is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Login attempt for locked out account: {Email}", model.Email);
                return BadRequest(new { message = "Your account has been locked due to too many failed attempts" });
            }

            // Get token from IdentityServer
            var tokenClient = new HttpClient();
            var identityServerUrl = _configuration["Authentication:Authority"];

            var disco = await tokenClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = identityServerUrl,
                Policy = { RequireHttps = !identityServerUrl.Contains("localhost") }
            });

            if (disco.IsError)
            {
                _logger.LogError("Error getting discovery document: {Error}", disco.Error);
                return StatusCode(500, new { message = "Error connecting to authentication service" });
            }

            // Request token using resource owner password grant
            var tokenResponse = await tokenClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "emtelaak_api_client",
                ClientSecret = "api_secret",
                UserName = model.Email,
                Password = model.Password,
                Scope = "openid profile email emtelaak_api offline_access"
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError("Error getting token: {Error}, {Description}",
                    tokenResponse.Error, tokenResponse.ErrorDescription);
                return Unauthorized(new { message = "Authentication failed" });
            }

            // If successful, reset the failed login count
            await _userManager.ResetAccessFailedCountAsync(user);

            return Ok(new
            {
                accessToken = tokenResponse.AccessToken,
                refreshToken = tokenResponse.RefreshToken,
                expiresIn = tokenResponse.ExpiresIn,
                tokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login process");
            return StatusCode(500, new { message = "An unexpected error occurred during login" });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
    {
        try
        {
            // Get token from IdentityServer
            var tokenClient = new HttpClient();
            var identityServerUrl = _configuration["Authentication:Authority"];

            var disco = await tokenClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = identityServerUrl,
                Policy = { RequireHttps = !identityServerUrl.Contains("localhost") }
            });

            if (disco.IsError)
            {
                _logger.LogError("Error getting discovery document: {Error}", disco.Error);
                return StatusCode(500, new { message = "Error connecting to authentication service" });
            }

            // Request a new token using the refresh token
            var tokenResponse = await tokenClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "emtelaak_api_client",
                ClientSecret = "api_secret",
                RefreshToken = model.RefreshToken
            });

            if (tokenResponse.IsError)
            {
                _logger.LogError("Error refreshing token: {Error}, {Description}",
                    tokenResponse.Error, tokenResponse.ErrorDescription);

                if (tokenResponse.Error == "invalid_grant")
                {
                    return Unauthorized(new { message = "Refresh token is invalid or expired" });
                }

                return StatusCode(500, new { message = "Error refreshing token" });
            }

            return Ok(new
            {
                accessToken = tokenResponse.AccessToken,
                refreshToken = tokenResponse.RefreshToken,
                expiresIn = tokenResponse.ExpiresIn,
                tokenType = "Bearer"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An unexpected error occurred during token refresh" });
        }
    }
}