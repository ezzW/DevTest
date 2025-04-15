// Emtelaak.UserRegistration.Application/Commands/SubmitKycVerificationCommandHandler.cs
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class SubmitKycVerificationCommandHandler : IRequestHandler<SubmitKycVerificationCommand, KycSubmissionResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IKycVerificationService _kycVerificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<SubmitKycVerificationCommandHandler> _logger;

        public SubmitKycVerificationCommandHandler(
            IUserRepository userRepository,
            IKycVerificationService kycVerificationService,
            IMapper mapper,
            ILogger<SubmitKycVerificationCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _kycVerificationService = kycVerificationService ?? throw new ArgumentNullException(nameof(kycVerificationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<KycSubmissionResultDto> Handle(SubmitKycVerificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Submitting KYC verification for user: {UserId}", request.UserId);

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    throw new ApplicationException($"User not found: {request.UserId}");
                }

                // Create KYC verification entity
                var kycVerification = _mapper.Map<KycVerification>(request.SubmissionData);

                // Submit to KYC service
                var submittedVerification = await _kycVerificationService.SubmitVerificationAsync(request.UserId, kycVerification);

                // Prepare result
                var result = new KycSubmissionResultDto
                {
                    VerificationId = submittedVerification.VerificationId,
                    Status = submittedVerification.Status.ToString(),
                    SubmittedAt = submittedVerification.SubmittedAt ?? DateTime.UtcNow
                };

                _logger.LogInformation("KYC verification submitted successfully for user: {UserId}, VerificationId: {VerificationId}",
                    request.UserId, result.VerificationId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting KYC verification: {Message}", ex.Message);
                throw;
            }
        }
    }
}