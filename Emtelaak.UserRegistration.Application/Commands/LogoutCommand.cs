// Emtelaak.UserRegistration.Application/Commands/LogoutCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class LogoutCommand : IRequest
    {
        public string RefreshToken { get; set; }
    }
}