// Emtelaak.UserRegistration.Application/Commands/DeleteKycDocumentCommandHandler.cs
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class DeleteKycDocumentCommandHandler : IRequestHandler<DeleteKycDocumentCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly ILogger<DeleteKycDocumentCommandHandler> _logger;

        public DeleteKycDocumentCommandHandler(
            IUserRepository userRepository,
            IDocumentStorageService documentStorageService,
            ILogger<DeleteKycDocumentCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentStorageService = documentStorageService ?? throw new ArgumentNullException(nameof(documentStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(DeleteKycDocumentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting KYC document: {DocumentId} for user: {UserId}",
                    request.DocumentId, request.UserId);

                // Get document
                var document = await _userRepository.GetDocumentByIdAsync(request.DocumentId);

                // Check if document exists
                if (document == null)
                {
                    _logger.LogWarning("Document not found: {DocumentId}", request.DocumentId);
                    throw new ApplicationException($"Document not found: {request.DocumentId}");
                }

                // Verify document belongs to the user
                if (document.UserId != request.UserId)
                {
                    _logger.LogWarning("Document {DocumentId} does not belong to user {UserId}",
                        request.DocumentId, request.UserId);
                    throw new ApplicationException("You do not have permission to delete this document");
                }

                // Verify document is not already verified
                if (document.VerificationStatus == DocumentVerificationStatus.Verified)
                {
                    _logger.LogWarning("Cannot delete verified document: {DocumentId}", request.DocumentId);
                    throw new ApplicationException("Verified documents cannot be deleted");
                }

                // Check if KYC verification is in progress
                var kycVerification = await _userRepository.GetKycVerificationByUserIdAsync(request.UserId);
                if (kycVerification != null &&
                    (kycVerification.Status == KycStatus.InProgress ||
                     kycVerification.Status == KycStatus.PendingReview))
                {
                    _logger.LogWarning("Cannot delete document during KYC verification: {DocumentId}", request.DocumentId);
                    throw new ApplicationException("Documents cannot be deleted while KYC verification is in progress");
                }

                // Delete document from storage
                var storagePath = document.StoragePath;
                await _documentStorageService.DeleteDocumentAsync(storagePath);

                // Delete document from database
                await _userRepository.DeleteDocumentAsync(request.DocumentId);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.DocumentUpload,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"documentId\":\"{document.Id}\",\"documentType\":\"{document.DocumentType}\",\"fileName\":\"{document.FileName}\",\"action\":\"delete\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                _logger.LogInformation("Document deleted successfully: {DocumentId}", request.DocumentId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting KYC document: {Message}", ex.Message);
                throw;
            }
        }
    }
}