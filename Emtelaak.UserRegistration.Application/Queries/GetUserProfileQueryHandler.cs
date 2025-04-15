// Emtelaak.UserRegistration.Application/Queries/GetUserProfileQueryHandler.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentStorageService _documentStorageService;
        private readonly IMapper _mapper;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            IUserRepository userRepository,
            IDocumentStorageService documentStorageService,
            IMapper mapper,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _documentStorageService = documentStorageService ?? throw new ArgumentNullException(nameof(documentStorageService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving profile for user ID: {UserId}", request.UserId);

                // Get user with related entities
                var user = await _userRepository.GetUserByIdWithDetailsAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found with ID: {request.UserId}");
                }

                // Map to DTO
                var profileDto = _mapper.Map<UserProfileDto>(user);

                // Add profile picture URL if exists
                var profilePictureDoc = user.Documents.FirstOrDefault(d => d.DocumentType == Domain.Enums.DocumentType.ProfilePicture);
                if (profilePictureDoc != null)
                {
                    profileDto.ProfilePictureUrl = _documentStorageService.GetDocumentUrl(profilePictureDoc.StoragePath);
                }

                // Calculate profile completion percentage
                profileDto.ProfileCompletionPercentage = CalculateProfileCompletionPercentage(user);

                _logger.LogInformation("Profile retrieved successfully for user ID: {UserId}", request.UserId);
                return profileDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile: {Message}", ex.Message);
                throw;
            }
        }

        private int CalculateProfileCompletionPercentage(Domain.Entities.User user)
        {
            int totalFields = 7; // Define how many fields contribute to completion
            int completedFields = 0;

            // Basic info
            if (!string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName))
                completedFields++;

            if (!string.IsNullOrEmpty(user.PhoneNumber) && user.PhoneVerified)
                completedFields++;

            if (!string.IsNullOrEmpty(user.Email) && user.EmailVerified)
                completedFields++;

            if (user.DateOfBirth.HasValue)
                completedFields++;

            if (!string.IsNullOrEmpty(user.CountryOfResidence))
                completedFields++;

            // KYC Verification
            if (user.KycVerification != null &&
                (user.KycVerification.Status == Domain.Enums.KycStatus.Approved ||
                 user.KycVerification.Status == Domain.Enums.KycStatus.InProgress))
                completedFields++;

            // Accreditation
            if (user.Accreditation != null &&
                (user.Accreditation.Status == Domain.Enums.AccreditationStatus.Approved ||
                 user.Accreditation.Status == Domain.Enums.AccreditationStatus.Pending))
                completedFields++;

            return (int)Math.Round((double)completedFields / totalFields * 100);
        }
    }
}