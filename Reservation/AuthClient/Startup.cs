using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.Extensions.Logging;

namespace AuthClient
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
           var builder = new ConfigurationBuilder();
           builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }
            Configuration = builder.Build();     
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {            
            IdentityModelEventSource.ShowPII = true;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })

            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
                options.SlidingExpiration = false;
            })
            .AddOpenIdConnect(options =>
            {
                // Note: these settings must match the application details
                // inserted in the database at the server level.
                options.ClientId = Configuration["Variables:ClientId"];
                options.ClientSecret = Configuration["Variables:ClientSecret"];
                options.SignedOutRedirectUri = Configuration["Variables:SignedOutRedirectUri"];

                options.RequireHttpsMetadata = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                // Use the authorization code flow.
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                // Note: setting the Authority allows the OIDC client middleware to automatically
                // retrieve the identity provider's configuration and spare you from setting
                // the different endpoints URIs or the token validation parameters explicitly.
                options.Authority = Configuration["Variables:Authority"];

                //options.Scope.Add("email");
                options.Scope.Add("roles");
                options.Scope.Add("reservations");
                options.Scope.Add("permis");
                options.Scope.Add("trips");

                /* options.SecurityTokenValidator = new JwtSecurityTokenHandler
                {
                    // Disable the built-in JWT claims mapping feature.
                    InboundClaimTypeMap = new Dictionary<string, string>()
                };
 */
                options.TokenValidationParameters.NameClaimType = "name";
                options.TokenValidationParameters.RoleClaimType = "role";
            });

            // Add authorization to validate the scopes before sending requests to APIs
            services.AddAuthorization(options =>
                {
                    options.AddPolicy("ApiScope", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        //policy.RequireClaim("scope", "roles");
                    });
                });

            services.AddControllersWithViews();

            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(options =>
            {
                options.MapControllers();//.RequireAuthorization("ApiScope");
                options.MapDefaultControllerRoute();
            });
        }
    }
}