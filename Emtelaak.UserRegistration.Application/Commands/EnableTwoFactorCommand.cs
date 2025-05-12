// Emtelaak.UserRegistration.Application/Commands/EnableTwoFactorCommand.cs
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class EnableTwoFactorCommand : IRequest<EnableTwoFactorResultDto>
    {
        public Guid UserId { get; set; }
        public string Method { get; set; }
        public bool Enable { get; set; }
    }
}