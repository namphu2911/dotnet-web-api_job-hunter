using JobHunter.Application.Contracts;

namespace JobHunter.Application.Abstractions;

public interface IWeatherForecastService
{
    IEnumerable<WeatherForecastDto> GetForecasts();
}
