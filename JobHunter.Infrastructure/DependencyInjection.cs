using JobHunter.Application.Abstractions;
using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Authentication;
using JobHunter.Infrastructure.Persistence;
using JobHunter.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);
        var serverVersion = ResolveServerVersion(configuration);

        services.AddDbContext<JobHunterDbContext>(options =>
            options.UseMySql(connectionString, serverVersion));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        return services;
    }

    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Dev fallback — override via user-secrets or appsettings.Development.json
            connectionString = "server=localhost;port=3306;database=jobhunter;user=root;password=123456";
        }

        return connectionString;
    }

    private static ServerVersion ResolveServerVersion(IConfiguration configuration)
    {
        var configured = configuration["Database:ServerVersion"];
        return !string.IsNullOrWhiteSpace(configured)
            ? ServerVersion.Parse(configured)
            : new MySqlServerVersion(new Version(8, 0, 36));
    }
}
