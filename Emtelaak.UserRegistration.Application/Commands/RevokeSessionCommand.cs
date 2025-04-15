// Emtelaak.UserRegistration.Application/Commands/RevokeSessionCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RevokeSessionCommand : IRequest
    {
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
        public string Reason { get; set; }
    }
}