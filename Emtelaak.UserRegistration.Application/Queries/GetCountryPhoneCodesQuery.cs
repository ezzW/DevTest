// Emtelaak.UserRegistration.Application/Queries/GetCountryPhoneCodesQuery.cs
using System.Collections.Generic;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetCountryPhoneCodesQuery : IRequest<List<CountryPhoneCodeDto>>
    {
        public string? SearchTerm { get; set; }
    }
}