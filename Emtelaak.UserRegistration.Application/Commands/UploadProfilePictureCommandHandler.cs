// Emtelaak.UserRegistration.Application/Commands/UploadProfilePictureCommandHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UploadProfilePictureCommandHandler : IRequestHandler<UploadProfilePictureCommand, DocumentUploadResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly ILogger<UploadProfilePictureCommandHandler> _logger;

        public UploadProfilePictureCommandHandler(
            IUserRepository userRepository,
            IDocumentStorageService documentStorageService,
            ILogger<UploadProfilePictureCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentStorageService = documentStorageService ?? throw new ArgumentNullException(nameof(documentStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DocumentUploadResultDto> Handle(UploadProfilePictureCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Uploading profile picture for user: {UserId}", request.UserId);

                // Get user
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // Check if user already has a profile picture
                var existingProfilePictures = await _userRepository.GetDocumentsByUserIdAsync(request.UserId);
                var existingProfilePicture = existingProfilePictures
                    .FirstOrDefault(d => d.DocumentType == DocumentType.ProfilePicture);

                // If there's an existing profile picture, delete it
                if (existingProfilePicture != null)
                {
                    _logger.LogInformation("Deleting existing profile picture: {DocumentId}", existingProfilePicture.Id);

                    // Delete from storage
                    await _documentStorageService.DeleteDocumentAsync(existingProfilePicture.StoragePath);

                    // Delete from database
                    await _userRepository.DeleteDocumentAsync(existingProfilePicture.Id);
                }

                // Upload new profile picture
                var storagePath = await _documentStorageService.UploadDocumentAsync(
                    request.File,
                    request.UserId,
                    "ProfilePicture");

                // Create document entity
                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    DocumentType = DocumentType.ProfilePicture,
                    FileName = request.File.FileName,
                    FileSize = (int)request.File.Length,
                    ContentType = request.File.ContentType,
                    StoragePath = storagePath,
                    UploadedAt = DateTime.UtcNow,
                    VerificationStatus = DocumentVerificationStatus.Verified // Auto-verify profile pictures
                };

                // Save document
                var savedDocument = await _userRepository.AddDocumentAsync(document);

                // Add activity log
                var activityLog = new ActivityLog
                {
                    UserId = request.UserId,
                    ActivityType = ActivityType.ProfileUpdate,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"profilePictureUpdated\":true}}"
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

                _logger.LogInformation("Profile picture uploaded successfully: {DocumentId}", result.DocumentId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture: {Message}", ex.Message);
                throw;
            }
        }
    }
}