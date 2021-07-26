using ReservationAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReservationAPI.Data;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace ReservationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ReservationAPI", Version = "v1" });
            });

            services.AddDbContext<ReservationContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });

            // Adding the authentication with OAuth 2.0
            services.AddAuthentication("Bearer")
                  .AddJwtBearer("Bearer", options =>
                  {
                      options.Authority = Configuration["Variables:Authority"];

                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateAudience = true,
                          ValidAudience = "SoukAOutilsReservations"
                      };
                  });

             services.AddAuthorization(options =>
                {
                    options.AddPolicy("ApiScope", policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        //policy.RequireClaim("scope", "roles");
                    });
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            // Supposed to be only if development
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReservationAPI v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(options =>
            {
                //options.MapControllers();

                // Adding the authentication with OAuth 2.0
                options.MapControllers().RequireAuthorization("ApiScope");
            });
        }
    }
}
