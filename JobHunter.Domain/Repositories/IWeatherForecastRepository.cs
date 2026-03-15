using JobHunter.Domain.Entities;

namespace JobHunter.Domain.Repositories;

public interface IWeatherForecastRepository
{
    IEnumerable<WeatherForecast> GetForecasts();
}
