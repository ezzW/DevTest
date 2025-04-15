// Emtelaak.UserRegistration.Application/Commands/ProcessKycWebhookCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ProcessKycWebhookCommand : IRequest<bool>
    {
        public string Payload { get; set; }
    }
}