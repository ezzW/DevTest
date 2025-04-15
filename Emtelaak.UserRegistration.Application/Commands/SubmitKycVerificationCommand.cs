// Emtelaak.UserRegistration.Application/Commands/SubmitKycVerificationCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class SubmitKycVerificationCommand : IRequest<KycSubmissionResultDto>
    {
        public Guid UserId { get; set; }
        public KycSubmissionDto SubmissionData { get; set; }
    }
}