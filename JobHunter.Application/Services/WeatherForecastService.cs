using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastRepository _weatherForecastRepository;

    public WeatherForecastService(IWeatherForecastRepository weatherForecastRepository)
    {
        _weatherForecastRepository = weatherForecastRepository;
    }

    public IEnumerable<WeatherForecastDto> GetForecasts()
    {
        return _weatherForecastRepository.GetForecasts().Select(forecast => new WeatherForecastDto
        {
            Date = forecast.Date,
            TemperatureC = forecast.TemperatureC,
            TemperatureF = forecast.TemperatureF,
            Summary = forecast.Summary
        });
    }
}
