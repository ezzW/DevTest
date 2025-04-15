// Emtelaak.UserRegistration.Application/Commands/ResendVerificationSmsCommand.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResendVerificationSmsCommand : IRequest
    {
        public string PhoneNumber { get; set; }
    }
}