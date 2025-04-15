// Emtelaak.UserRegistration.Application/Queries/GetKycDocumentQueryHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetKycDocumentQueryHandler : IRequestHandler<GetKycDocumentQuery, DocumentDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly ILogger<GetKycDocumentQueryHandler> _logger;

        public GetKycDocumentQueryHandler(
            IUserRepository userRepository,
            IDocumentStorageService documentStorageService,
            ILogger<GetKycDocumentQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentStorageService = documentStorageService ?? throw new ArgumentNullException(nameof(documentStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DocumentDto> Handle(GetKycDocumentQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting KYC document: {DocumentId} for user: {UserId}",
                    request.DocumentId, request.UserId);

                // Get document
                var document = await _userRepository.GetDocumentByIdAsync(request.DocumentId);

                // Check if document exists
                if (document == null)
                {
                    _logger.LogWarning("Document not found: {DocumentId}", request.DocumentId);
                    return null;
                }

                // Verify document belongs to the user
                if (document.UserId != request.UserId)
                {
                    _logger.LogWarning("Document {DocumentId} does not belong to user {UserId}",
                        request.DocumentId, request.UserId);
                    throw new ApplicationException("You do not have permission to access this document");
                }

                // Map to DTO
                var documentDto = new DocumentDto
                {
                    Id = document.Id,
                    DocumentType = document.DocumentType.ToString(),
                    FileName = document.FileName,
                    UploadedAt = document.UploadedAt,
                    Status = document.VerificationStatus.ToString(),
                    RejectionReason = document.RejectionReason,
                    FileUrl = _documentStorageService.GetDocumentUrl(document.StoragePath)
                };

                _logger.LogInformation("Retrieved document: {DocumentId} for user: {UserId}",
                    request.DocumentId, request.UserId);

                return documentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC document: {Message}", ex.Message);
                throw;
            }
        }
    }
}