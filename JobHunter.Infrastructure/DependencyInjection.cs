using JobHunter.Domain.Repositories;
using JobHunter.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();
        return services;
    }
}
