using Emtelaak.UserRegistration.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/diagnostic")]
public class DiagnosticController : ControllerBase
{
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(ILogger<DiagnosticController> logger)
    {
        _logger = logger;
    }

    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This is a public endpoint", timestamp = DateTime.UtcNow });
    }

    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult AuthenticatedEndpoint()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        _logger.LogInformation("User accessed authenticated endpoint: {UserId}", userIdClaim);

        return Ok(new
        {
            message = "Authentication successful",
            userId = userIdClaim,
            claims = claims,
            timestamp = DateTime.UtcNow
        });
    }
}