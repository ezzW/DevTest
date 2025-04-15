// Emtelaak.UserRegistration.Application/Commands/LoginUserCommand.cs
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class LoginUserCommand : IRequest<LoginResultDto>
    {
        public LoginDto LoginData { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
