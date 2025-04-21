// Emtelaak.UserRegistration.Application/Queries/GetCountriesQueryHandler.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Emtelaak.UserRegistration.Application.DTOs;
using Emtelaak.UserRegistration.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emtelaak.UserRegistration.Application.Queries
{
    /// <summary>
    /// Handler for GetCountriesQuery
    /// </summary>
    public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, List<CountryDto>>
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCountriesQueryHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="countryRepository">Country repository</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        public GetCountriesQueryHandler(
            ICountryRepository countryRepository,
            IMapper mapper,
            ILogger<GetCountriesQueryHandler> logger)
        {
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle the query to get all countries
        /// </summary>
        /// <param name="request">Query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of countries</returns>
        public async Task<List<CountryDto>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetCountriesQuery");

                if (string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    // Get all active countries
                    var countries = await _countryRepository.GetActiveCountriesAsync();
                    return _mapper.Map<List<CountryDto>>(countries);
                }
                else
                {
                    // Search countries with the provided term
                    var countries = await _countryRepository.SearchCountriesAsync(request.SearchTerm);
                    return _mapper.Map<List<CountryDto>>(countries);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting countries: {Message}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Handler for GetCountryPhoneCodesQuery
    /// </summary>
    public class GetCountryPhoneCodesQueryHandler : IRequestHandler<GetCountryPhoneCodesQuery, List<CountryPhoneCodeDto>>
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCountryPhoneCodesQueryHandler> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="countryRepository">Country repository</param>
        /// <param name="mapper">AutoMapper instance</param>
        /// <param name="logger">Logger</param>
        public GetCountryPhoneCodesQueryHandler(
            ICountryRepository countryRepository,
            IMapper mapper,
            ILogger<GetCountryPhoneCodesQueryHandler> logger)
        {
            _countryRepository = countryRepository ?? throw new ArgumentNullException(nameof(countryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Handle the query to get all country phone codes
        /// </summary>
        /// <param name="request">Query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of country phone codes</returns>
        public async Task<List<CountryPhoneCodeDto>> Handle(GetCountryPhoneCodesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetCountryPhoneCodesQuery");

                List<Domain.Entities.Country> countries;

                if (string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    // Get all active countries
                    countries = await _countryRepository.GetActiveCountriesAsync();
                }
                else
                {
                    // Search countries with the provided term
                    countries = await _countryRepository.SearchCountriesAsync(request.SearchTerm);
                }

                // Map to DTOs and add + prefix to phone codes
                var phoneCodeDtos = _mapper.Map<List<CountryPhoneCodeDto>>(countries);

                // Add + prefix to phone codes if not already present
                foreach (var dto in phoneCodeDtos)
                {
                    if (!dto.PhoneCode.StartsWith("+"))
                    {
                        dto.PhoneCode = "+" + dto.PhoneCode;
                    }
                }

                return phoneCodeDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting country phone codes: {Message}", ex.Message);
                throw;
            }
        }
    }
}