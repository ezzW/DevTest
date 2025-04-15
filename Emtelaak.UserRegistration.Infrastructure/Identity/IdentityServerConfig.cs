// Emtelaak.UserRegistration.Infrastructure/Identity/ApplicationUser.cs
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.Extensions.Configuration;

namespace Emtelaak.UserRegistration.Infrastructure.Identity
{
    public static class IdentityServerConfig
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone()
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("emtelaak_api", "Emtelaak API")
                {
                    Scopes = { "emtelaak_api" }
                }
            };
        }

        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("emtelaak_api", "Full access to Emtelaak API")
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new List<Client>
            {
                new Client
                {
            ClientId = "emtelaak_web_client",
            ClientName = "Emtelaak Web Client",
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = false,
            RequirePkce = true,

            RedirectUris = { configuration["Authentication:WebClientRedirectUri"] },
            PostLogoutRedirectUris = { configuration["Authentication:WebClientPostLogoutRedirectUri"] },
            AllowedCorsOrigins = { configuration["Authentication:WebClientCorsOrigin"] },

            AllowedScopes =
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,
                "emtelaak_api"
            },
            AllowOfflineAccess = true,
            AccessTokenLifetime = 3600, // 1 hour
            RefreshTokenUsage = TokenUsage.OneTimeOnly,
            RefreshTokenExpiration = TokenExpiration.Absolute,
            AbsoluteRefreshTokenLifetime = 86400 * 14, // 14 days
        }
    };
        }
    }
}