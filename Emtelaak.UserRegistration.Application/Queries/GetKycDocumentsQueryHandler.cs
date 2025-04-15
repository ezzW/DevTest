// Emtelaak.UserRegistration.Application/Queries/GetKycDocumentsQueryHandler.cs
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetKycDocumentsQueryHandler : IRequestHandler<GetKycDocumentsQuery, List<DocumentDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly ILogger<GetKycDocumentsQueryHandler> _logger;

        public GetKycDocumentsQueryHandler(
            IUserRepository userRepository,
            IDocumentStorageService documentStorageService,
            ILogger<GetKycDocumentsQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentStorageService = documentStorageService ?? throw new ArgumentNullException(nameof(documentStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DocumentDto>> Handle(GetKycDocumentsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting KYC documents for user: {UserId}", request.UserId);

                // Get documents for user
                var documents = await _userRepository.GetDocumentsByUserIdAsync(request.UserId);

                // Filter KYC-related documents (exclude profile picture, etc.)
                var kycDocuments = documents.Where(d =>
                    d.DocumentType != DocumentType.ProfilePicture).ToList();

                // Map to DTOs
                var documentDtos = kycDocuments.Select(doc => new DocumentDto
                {
                    Id = doc.Id,
                    DocumentType = doc.DocumentType.ToString(),
                    FileName = doc.FileName,
                    UploadedAt = doc.UploadedAt,
                    Status = doc.VerificationStatus.ToString(),
                    RejectionReason = doc.RejectionReason,
                    FileUrl = _documentStorageService.GetDocumentUrl(doc.StoragePath)
                }).ToList();

                _logger.LogInformation("Retrieved {Count} KYC documents for user: {UserId}",
                    documentDtos.Count, request.UserId);

                return documentDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving KYC documents: {Message}", ex.Message);
                throw;
            }
        }
    }
}