// Emtelaak.UserRegistration.Application/Commands/RefreshTokenCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class RefreshTokenCommand : IRequest<LoginResultDto>
    {
        public string RefreshToken { get; set; }
    }
}