// Emtelaak.UserRegistration.Application/Commands/VerifyTwoFactorSetupCommand.cs
using MediatR;

namespace Emtelaak.UserRegistration.Application.Commands
{
    public class VerifyTwoFactorSetupCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string Method { get; set; }
        public string VerificationCode { get; set; }
    }
}