// Emtelaak.UserRegistration.Application/Queries/GetKycStatusQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetKycStatusQuery : IRequest<KycStatusDto>
    {
        public Guid UserId { get; set; }
    }
}