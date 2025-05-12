// Emtelaak.UserRegistration.Application/Commands/ResendMfaCodeCommand.cs
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class ResendMfaCodeCommand : IRequest<ResendMfaCodeResultDto>
    {
        public string MfaToken { get; set; }
        public string Method { get; set; }
    }
}
