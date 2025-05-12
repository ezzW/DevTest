// Emtelaak.UserRegistration.Application/Commands/UpdateAccreditationStatusCommandHandler.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using Emtelaak.UserRegistration.Domain.Entities;
using Emtelaak.UserRegistration.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class UpdateAccreditationStatusCommandHandler : IRequestHandler<UpdateAccreditationStatusCommand, UpdateAccreditationStatusResultDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<UpdateAccreditationStatusCommandHandler> _logger;

        public UpdateAccreditationStatusCommandHandler(
            IUserRepository userRepository,
            IMapper mapper,
            IEmailService emailService,
            ILogger<UpdateAccreditationStatusCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UpdateAccreditationStatusResultDto> Handle(UpdateAccreditationStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating accreditation status for ID: {AccreditationId}", request.AccreditationId);

                // Get accreditation
                var user = await _userRepository.GetUserByIdWithDetailsAsync(request.AdminUserId);
                if (user == null)
                {
                    throw new ApplicationException($"Admin user with ID {request.AdminUserId} not found");
                }

                // Check if admin has proper role or permissions (would be implemented in a real system)
                // This is a placeholder for role/permission check
                
                // Get the accreditation
                var accreditation = await _userRepository.GetAccreditationByIdAsync(request.AccreditationId);
                if (accreditation == null)
                {
                    throw new ApplicationException($"Accreditation with ID {request.AccreditationId} not found");
                }

                // Get user who owns the accreditation
                var accreditationUser = await _userRepository.GetUserByIdWithDetailsAsync(accreditation.UserId);
                if (accreditationUser == null)
                {
                    throw new ApplicationException($"User associated with accreditation ID {request.AccreditationId} not found");
                }

                // Update status
                var newStatus = Enum.Parse<AccreditationStatus>(request.StatusData.Status);
                var oldStatus = accreditation.Status;
                accreditation.Status = newStatus;
                accreditation.ReviewNotes = request.StatusData.ReviewNotes;
                accreditation.LastUpdatedAt = DateTime.UtcNow;

                // Set specific fields based on status
                if (newStatus == AccreditationStatus.Approved)
                {
                    accreditation.ApprovedAt = DateTime.UtcNow;
                    accreditation.ApprovedBy = request.AdminUserId;
                    accreditation.ExpiresAt = request.StatusData.ExpiresAt ?? DateTime.UtcNow.AddYears(1); // Default to 1 year expiry
                    
                    // Use the admin-specified limit if provided, otherwise calculate based on classification
                    if (request.StatusData.InvestmentLimitAmount.HasValue)
                    {
                        accreditation.InvestmentLimitAmount = request.StatusData.InvestmentLimitAmount;
                        accreditation.OverrideEnabled = true;
                        accreditation.OverrideBy = request.AdminUserId;
                    }
                    else
                    {
                        // Calculate limit based on investor classification and financial information
                        accreditation.InvestmentLimitAmount = Services.InvestmentLimitService.CalculateInvestmentLimit(accreditation);
                        accreditation.OverrideEnabled = false;
                        accreditation.OverrideBy = null;
                    }
                }

                // Save changes
                await _userRepository.UpdateAccreditationAsync(accreditation);

                // Log the activity
                var activityLog = new ActivityLog
                {
                    UserId = accreditation.UserId,
                    ActivityType = ActivityType.AccreditationStatusUpdated,
                    Timestamp = DateTime.UtcNow,
                    Status = ActivityStatus.Success,
                    Details = $"{{\"oldStatus\":\"{oldStatus}\",\"newStatus\":\"{newStatus}\",\"updatedBy\":\"{request.AdminUserId}\"}}"
                };
                await _userRepository.AddActivityLogAsync(activityLog);

                // Send email notification based on new status
                try
                {
                    if (newStatus == AccreditationStatus.Approved)
                    {
                        await SendAccreditationApprovedEmailAsync(accreditationUser.Email, accreditationUser.FirstName);
                    }
                    else if (newStatus == AccreditationStatus.Rejected)
                    {
                        await SendAccreditationRejectedEmailAsync(accreditationUser.Email, accreditationUser.FirstName, request.StatusData.ReviewNotes);
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the operation if email fails
                    _logger.LogError(ex, "Error sending accreditation notification email: {Message}", ex.Message);
                }

                // Return result
                return new UpdateAccreditationStatusResultDto
                {
                    AccreditationId = accreditation.Id,
                    Status = accreditation.Status.ToString(),
                    InvestorClassification = accreditation.InvestorClassification.ToString(),
                    ApprovedAt = accreditation.ApprovedAt,
                    ExpiresAt = accreditation.ExpiresAt,
                    InvestmentLimitAmount = accreditation.InvestmentLimitAmount,
                    ReviewNotes = accreditation.ReviewNotes,
                    Successful = true,
                    Message = "Accreditation status updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating accreditation status: {Message}", ex.Message);
                return new UpdateAccreditationStatusResultDto
                {
                    Successful = false,
                    Message = $"Error updating accreditation status: {ex.Message}"
                };
            }
        }

        private async Task SendAccreditationApprovedEmailAsync(string email, string name)
        {
            try
            {
                await _emailService.SendAccreditationApprovedEmailAsync(email, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending accreditation approved email: {Message}", ex.Message);
                throw;
            }
        }

        private async Task SendAccreditationRejectedEmailAsync(string email, string name, string reason)
        {
            try
            {
                await _emailService.SendAccreditationRejectedEmailAsync(email, name, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending accreditation rejected email: {Message}", ex.Message);
                throw;
            }
        }
    }
}