// Emtelaak.UserRegistration.API/Controllers/AccreditationController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.Commands;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.API.Controllers
{
    [ApiController]
    [Route("api/accreditation")]
    public class AccreditationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccreditationController> _logger;
        private readonly IDocumentRequirementsService _documentRequirementsService;

        public AccreditationController(
            IMediator mediator,
            ILogger<AccreditationController> logger,
            IDocumentRequirementsService documentRequirementsService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _documentRequirementsService = documentRequirementsService ?? throw new ArgumentNullException(nameof(documentRequirementsService));
        }

        [HttpGet("status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AccreditationStatusDto>> GetAccreditationStatus()
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var query = new GetAccreditationStatusQuery { UserId = userId };
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error getting accreditation status: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("submit")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<SubmitAccreditationResultDto>> SubmitAccreditation([FromBody] SubmitAccreditationDto submitDto)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var command = new SubmitAccreditationCommand
                {
                    UserId = userId,
                    AccreditationData = submitDto
                };

                var result = await _mediator.Send(command);

                if (!result.Successful)
                {
                    return BadRequest(result);
                }

                return Accepted(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error submitting accreditation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("update-status/{accreditationId}")]
        [Authorize(Roles = "Admin")] // Only admins can update status
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UpdateAccreditationStatusResultDto>> UpdateAccreditationStatus(
            Guid accreditationId, 
            [FromBody] UpdateAccreditationStatusDto statusDto)
        {
            try
            {
                var adminUserId = GetUserIdFromClaims();

                var command = new UpdateAccreditationStatusCommand
                {
                    AdminUserId = adminUserId,
                    AccreditationId = accreditationId,
                    StatusData = statusDto
                };

                var result = await _mediator.Send(command);

                if (!result.Successful)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error updating accreditation status: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("documents")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentUploadResultDto>> UploadAccreditationDocument(IFormFile file, [FromForm] string documentType)
        {
            try
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
                    DocumentType = documentType,
                    IsAccreditationDocument = true
                };

                var result = await _mediator.Send(command);

                return Created($"/api/accreditation/documents/{result.DocumentId}", result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error uploading accreditation document: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("documents")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<DocumentDto>>> GetAccreditationDocuments()
        {
            try 
            {
                var userId = GetUserIdFromClaims();

                var query = new GetKycDocumentsQuery 
                { 
                    UserId = userId,
                    ForAccreditation = true
                };
                
                var result = await _mediator.Send(query);

                return Ok(result);
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Error getting accreditation documents: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("documents/{documentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<DocumentDto>> GetAccreditationDocument(Guid documentId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var query = new GetKycDocumentQuery
                {
                    UserId = userId,
                    DocumentId = documentId,
                    ForAccreditation = true
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
                _logger.LogWarning(ex, "Document not found or access denied: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("documents/{documentId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAccreditationDocument(Guid documentId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                var command = new DeleteKycDocumentCommand
                {
                    UserId = userId,
                    DocumentId = documentId,
                    ForAccreditation = true
                };

                await _mediator.Send(command);

                return NoContent();
            }
            catch (ApplicationException ex)
            {
                _logger.LogWarning(ex, "Document not found or access denied: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("document-requirements")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<RequiredDocumentDto>>> GetAccreditationDocumentRequirements([FromQuery] string investorClassification = null)
        {
            try
            {
                var requirements = string.IsNullOrEmpty(investorClassification)
                    ? await _documentRequirementsService.GetAllAccreditationDocumentRequirementsAsync()
                    : await _documentRequirementsService.GetAccreditationDocumentRequirementsAsync(investorClassification);

                return Ok(requirements);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting accreditation document requirements: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
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