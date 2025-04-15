// Emtelaak.UserRegistration.Application/Commands/VerifyEmailCommand.cs
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyEmailCommand : IRequest<EmailVerificationResultDto>
    {
        public EmailVerificationDto VerificationData { get; set; }
    }
}