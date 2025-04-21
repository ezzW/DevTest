// Emtelaak.UserRegistration.Application/Queries/GetCountriesQuery.cs
using System.Collections.Generic;
using Emtelaak.UserRegistration.Application.DTOs;
using MediatR;

namespace Emtelaak.UserRegistration.Application.Queries
{
    public class GetCountriesQuery : IRequest<List<CountryDto>>
    {
        public string? SearchTerm { get; set; }
    }
}