// Emtelaak.UserRegistration.Application/Commands/RegisterUserCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RegisterUserCommand : IRequest<UserRegistrationResultDto>
    {
        public UserRegistrationDto RegistrationData { get; set; }
    }
}

