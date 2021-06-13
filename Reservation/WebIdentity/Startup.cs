using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using WebIdentity.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.AspNetCore.Identity.UI.Services;
using WebIdentity.Models;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace WebIdentity
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/identity/account/login";
                })
                
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["GoogleSoukAOutilsApp:ClientID"];
                    options.ClientSecret = Configuration["GoogleSoukAOutilsApp:ClientSecret"];
                    options.SaveTokens = true;
                }) 
                .AddOpenIdConnect("AzureOpenId", "Microsoft", o =>
                 {
                    o.Authority = "https://login.microsoftonline.com/organizations/v2.0/";
                    o.ClientId = Configuration["AzureSoukAOutilsApp:ClientID"];
                    o.ClientSecret = Configuration["AzureSoukAOutilsApp:ClientSecret"];
                    o.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    o.CallbackPath = "/signin-azuread-oidc";
                    o.RequireHttpsMetadata = false;
                    o.SaveTokens = true;
                    o.GetClaimsFromUserInfoEndpoint = true;
                    o.Scope.Add("email");
                    o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false
                    };

                    //o.ClaimActions.MapJsonKey("sub", "sub");
                 })
                .AddOpenIdConnect("oidc", "Identité Ville", options =>
                {
                    options.Authority = "https://localhost:5003";

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    //options.Resource.

                    options.SaveTokens = true;
                    options.Scope.Add("api1");
                    options.Scope.Add("email");
                    //options.Scope.Add("role");
                    options.Scope.Add("roles"); // this is required to get the role in the access token. IsInRole on the MVC side which has access to the Id Token
                    options.Scope.Add("idRoles"); // this is required to get the role in the id token [Authorize(Roles=xxx)] and IsInRole in the API side which only have access to the access token
                    options.ClaimActions.MapUniqueJsonKey("role", "role");
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {RoleClaimType = "role"};

                    options.Events = new OpenIdConnectEvents 
                    {
                        OnRedirectToIdentityProvider = context => { 
                            context.ProtocolMessage.SetParameter("audience","https://my/api");
                            return Task.CompletedTask;

                        }
                    };

                });                
            
            services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(
                                        Configuration.GetConnectionString("DefaultConnection"));
                    options.UseOpenIddict();
                });
            
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            
            services.AddOpenIddict()
                    // Register the OpenIddict core components.
                    .AddCore(options =>
                    {
                        // Configure OpenIddict to use the EF Core stores/models.
                        options.UseEntityFrameworkCore()
                            .UseDbContext<ApplicationDbContext>();
                    })

                    // Register the OpenIddict server components.
                    .AddServer(options =>
                    {
                        
                        options
                            .AllowClientCredentialsFlow()
                            .AllowAuthorizationCodeFlow()
                                .RequireProofKeyForCodeExchange()
                            .AllowRefreshTokenFlow();

                        options
                            .SetAuthorizationEndpointUris("/connect/authorize")
                            .SetTokenEndpointUris("/connect/token")
                            .SetLogoutEndpointUris("/connect/logout")
                            .SetUserinfoEndpointUris("/connect/userinfo");

                        // Encryption and signing of tokens
                        options
                            .AddEphemeralEncryptionKey()
                            .AddEphemeralSigningKey()
                            .DisableAccessTokenEncryption();

                        // Register scopes (permissions)
                        options.RegisterScopes(
                            Scopes.Profile,
                            Scopes.Email, 
                            "api", "api1", "roles", "role");

                        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                        options
                            .UseAspNetCore()
                            .EnableTokenEndpointPassthrough()
                            .EnableAuthorizationEndpointPassthrough()
                            .EnableLogoutEndpointPassthrough()
                            .EnableUserinfoEndpointPassthrough();    

                    });
                services.AddSingleton<IEmailSender, EmailSender>();
                services.AddHostedService<Worker>();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
