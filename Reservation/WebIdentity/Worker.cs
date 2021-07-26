using System;
using System.Threading;
using System.Threading.Tasks;
using WebIdentity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace WebIdentity
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public Worker(IServiceProvider serviceProvider, IConfiguration configuration)
        {            
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            if (await manager.FindByClientIdAsync("postman", cancellationToken) is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    
                    ClientId = "postman",
                    ClientSecret = "postman-secret",
                    DisplayName = "Postman",
                    RedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") },
                    Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Authorization,
                        OpenIddictConstants.Permissions.Endpoints.Token,

                        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                        OpenIddictConstants.Permissions.GrantTypes.Implicit,
                        OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdToken,
                        OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken,
                        OpenIddictConstants.Permissions.ResponseTypes.Token,
                        OpenIddictConstants.Permissions.Scopes.Email,
                        OpenIddictConstants.Permissions.Scopes.Profile,
                        OpenIddictConstants.Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "role",

                        OpenIddictConstants.Permissions.ResponseTypes.Code                    
                    },
                }, 
                cancellationToken);
            }

            if (await manager.FindByClientIdAsync("mvc") == null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "mvc",
                    ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                    ConsentType = ConsentTypes.Explicit,
                    DisplayName = "SoukAOutils",
                    PostLogoutRedirectUris =
                    {
                        new Uri(_configuration["Variables:Client_URL"] + "/signout-callback-oidc")
                    },
                    RedirectUris =
                    {
                        new Uri(_configuration["Variables:Client_URL"] + "/signin-oidc")
                    },
                    Permissions =
                    {
                        Permissions.Endpoints.Authorization,
                        Permissions.Endpoints.Logout,
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.AuthorizationCode,
                        Permissions.GrantTypes.RefreshToken,
                        Permissions.ResponseTypes.Code,
                        Permissions.Scopes.Email,
                        Permissions.Scopes.Profile,
                        Permissions.Scopes.Roles,
                        OpenIddictConstants.Permissions.Prefixes.Scope + "trips",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "roles",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "reservations",
                        OpenIddictConstants.Permissions.Prefixes.Scope + "role",
                    }/* ,
                    Requirements =
                    {
                        Requirements.Features.ProofKeyForCodeExchange
                    } */
                }, cancellationToken);
            }
            
            await RegisterScopesAsync(scope.ServiceProvider);
            static async Task RegisterScopesAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("demo_api") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "Demo API access",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Accès à l'API de démo"
                        },
                        Name = "demo_api",
                        Resources =
                        {
                            "resource_server"
                        }
                    });
                }
                if (await manager.FindByNameAsync("reservations") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "Reservations Access",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Accès aux réservations"
                        },
                        Name = "reservations",
                        Resources =
                        {
                            "SoukAOutilsReservations"
                        }
                    });
                }

            }     
            
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}