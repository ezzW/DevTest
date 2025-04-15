// Emtelaak.UserRegistration.Application/Commands/VerifyMfaCommand.cs
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyMfaCommand : IRequest<LoginResultDto>
    {
        public MfaVerificationDto VerificationData { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}