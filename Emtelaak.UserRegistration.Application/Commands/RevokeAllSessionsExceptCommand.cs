// Emtelaak.UserRegistration.Application/Commands/RevokeAllSessionsExceptCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RevokeAllSessionsExceptCommand : IRequest
    {
        public Guid UserId { get; set; }
        public Guid ActiveSessionId { get; set; }
        public string Reason { get; set; }
    }
}