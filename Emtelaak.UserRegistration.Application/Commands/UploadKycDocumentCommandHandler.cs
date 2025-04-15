// Emtelaak.UserRegistration.Application/Commands/UploadKycDocumentCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UploadKycDocumentCommandHandler : IRequestHandler<UploadKycDocumentCommand, DocumentUploadResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly ILogger<UploadKycDocumentCommandHandler> _logger;

        public UploadKycDocumentCommandHandler(
            IUserRepository userRepository,
            IDocumentStorageService documentStorageService,
            ILogger<UploadKycDocumentCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentStorageService = documentStorageService ?? throw new ArgumentNullException(nameof(documentStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DocumentUploadResultDto> Handle(UploadKycDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Uploading KYC document for user: {UserId}, Document type: {DocumentType}",
                    request.UserId, request.DocumentType);

                // Validate user
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // Validate document type
                if (!Enum.TryParse<DocumentType>(request.DocumentType, true, out var docType))
                {
                    _logger.LogWarning("Invalid document type: {DocumentType}", request.DocumentType);
                    throw new ArgumentException($"Invalid document type: {request.DocumentType}");
                }

                // Upload file to storage
                var storagePath = await _documentStorageService.UploadDocumentAsync(
                    request.File,
                    request.UserId,
                    request.DocumentType);

                // Create document entity
                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    DocumentType = docType,
                    FileName = request.File.FileName,
                    FileSize = (int)request.File.Length,
                    ContentType = request.File.ContentType,
                    StoragePath = storagePath,
                    UploadedAt = DateTime.UtcNow,
                    VerificationStatus = DocumentVerificationStatus.Pending
                };

                // Save document in database
                var savedDocument = await _userRepository.AddDocumentAsync(document);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.DocumentUpload,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"documentId\":\"{savedDocument.Id}\",\"documentType\":\"{docType}\",\"fileName\":\"{savedDocument.FileName}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                // Prepare result
                var result = new DocumentUploadResultDto
                {
                    DocumentId = savedDocument.Id,
                    DocumentType = savedDocument.DocumentType.ToString(),
                    FileName = savedDocument.FileName,
                    UploadedAt = savedDocument.UploadedAt,
                    Status = savedDocument.VerificationStatus.ToString()
                };

                _logger.LogInformation("Document uploaded successfully: {DocumentId}", result.DocumentId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading KYC document: {Message}", ex.Message);
                throw;
            }
        }
    }
}