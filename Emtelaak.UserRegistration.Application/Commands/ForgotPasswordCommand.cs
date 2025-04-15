// Emtelaak.UserRegistration.Application/Commands/ForgotPasswordCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ForgotPasswordCommand : IRequest<ForgotPasswordResultDto>
    {
        public string Email { get; set; }
    }
}