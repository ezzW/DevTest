// Emtelaak.UserRegistration.Application/Queries/GetTwoFactorStatusQuery.cs
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetTwoFactorStatusQuery : IRequest<TwoFactorStatusDto>
    {
        public Guid UserId { get; set; }
    }
}