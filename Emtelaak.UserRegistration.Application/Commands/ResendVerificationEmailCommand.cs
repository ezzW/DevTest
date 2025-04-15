// Emtelaak.UserRegistration.Application/Commands/ResendVerificationEmailCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResendVerificationEmailCommand : IRequest
    {
        public string Email { get; set; }
    }
}