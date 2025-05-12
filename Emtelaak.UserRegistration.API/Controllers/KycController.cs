// Emtelaak.UserRegistration.API/Controllers/KycController.cs
using System;
using System.Collections.Generic;
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
    [Route("api/kyc")]
    [Authorize]
    public class KycController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<KycController> _logger;

        public KycController(
            IMediator mediator,
            ILogger<KycController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<KycStatusDto>> GetKycStatus()
        {
                var userId = GetUserIdFromClaims();

                var query = new GetKycStatusQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(result);
            
        }

        [HttpPost("documents")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentUploadResultDto>> UploadDocument(IFormFile file, [FromForm] string documentType)
        {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { errors = new { file = new[] { "No file uploaded" } } });
                }

                if (string.IsNullOrWhiteSpace(documentType))
                {
                    return BadRequest(new { errors = new { documentType = new[] { "Document type is required" } } });
                }

                // Check file size (limit to 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { errors = new { file = new[] { "File size exceeds maximum allowed (10MB)" } } });
                }

                // Check file type
                var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png" };
                if (!Array.Exists(allowedTypes, type => type == file.ContentType))
                {
                    return BadRequest(new { errors = new { file = new[] { "Invalid file type. Only PDF, JPEG and PNG are supported." } } });
                }

                var userId = GetUserIdFromClaims();

                var command = new UploadKycDocumentCommand
                {
                    UserId = userId,
                    File = file,
                    DocumentType = documentType
                };

                var result = await _mediator.Send(command);

                return Created($"/api/kyc/documents/{result.DocumentId}", result);

        }

        [HttpPost("submit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<KycSubmissionResultDto>> SubmitKyc([FromBody] KycSubmissionDto kycSubmissionDto)
        {
                var userId = GetUserIdFromClaims();

                var command = new SubmitKycVerificationCommand
                {
                    UserId = userId,
                    SubmissionData = kycSubmissionDto
                };

                var result = await _mediator.Send(command);

                return Accepted(result);
        }

        [HttpGet("documents")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<DocumentDto>>> GetKycDocuments()
        {
                var userId = GetUserIdFromClaims();

                var query = new GetKycDocumentsQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(result);            
        }

        [HttpGet("documents/{documentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentDto>> GetKycDocument(Guid documentId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var query = new GetKycDocumentQuery
                {
                    UserId = userId,
                    DocumentId = documentId
                };

                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Document not found or access denied");
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("documents/{documentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteKycDocument(Guid documentId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var command = new DeleteKycDocumentCommand
                {
                    UserId = userId,
                    DocumentId = documentId
                };

                await _mediator.Send(command);

                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Document not found or access denied");
                return NotFound(new { message = ex.Message });
            }
        }

        // Webhook endpoint for KYC provider callbacks - not authenticated
        [HttpPost("webhook")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> KycWebhook([FromBody] object webhookPayload)
        {
            try
            {
                var command = new ProcessKycWebhookCommand { Payload = webhookPayload.ToString() };
                await _mediator.Send(command);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing KYC webhook");
                return BadRequest(new { message = "Error processing webhook" });
            }
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst("sub")?.Value??
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