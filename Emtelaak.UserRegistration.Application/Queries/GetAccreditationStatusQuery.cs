// Emtelaak.UserRegistration.Application/Queries/GetAccreditationStatusQuery.cs
using System;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetAccreditationStatusQuery : IRequest<AccreditationStatusDto>
    {
        public Guid UserId { get; set; }
    }
}