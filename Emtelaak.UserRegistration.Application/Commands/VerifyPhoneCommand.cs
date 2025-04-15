// Emtelaak.UserRegistration.Application/Commands/VerifyPhoneCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Domain.Models;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyPhoneCommand : IRequest<PhoneVerificationResultDto>
    {
        public PhoneVerificationDto VerificationData { get; set; }
    }
}



