using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Middlewares
{
    public static class CorsConfig
    {
        private const string PolicyName = "PortafolioBackendCorsPolicy";

        public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration config)
        {
            var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            services.AddCors(options =>
                options.AddPolicy(PolicyName, policy =>
                    policy.WithOrigins(origins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()));

            return services;
        }

        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
            => app.UseCors(PolicyName);
    }
}