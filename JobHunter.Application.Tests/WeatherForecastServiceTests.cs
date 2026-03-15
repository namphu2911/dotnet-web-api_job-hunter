using JobHunter.Application.Services;
using JobHunter.Domain.Entities;
using JobHunter.Domain.Repositories;

namespace JobHunter.Application.Tests;

public class WeatherForecastServiceTests
{
    [Fact]
    public void GetForecasts_MapsDomainEntityToDto()
    {
        var repository = new FakeWeatherForecastRepository();
        var service = new WeatherForecastService(repository);

        var result = service.GetForecasts().ToList();

        Assert.Single(result);
        Assert.Equal(new DateOnly(2026, 3, 15), result[0].Date);
        Assert.Equal(20, result[0].TemperatureC);
        Assert.Equal(67, result[0].TemperatureF);
        Assert.Equal("Mild", result[0].Summary);
    }

    private sealed class FakeWeatherForecastRepository : IWeatherForecastRepository
    {
        public IEnumerable<WeatherForecast> GetForecasts()
        {
            return new[]
            {
                new WeatherForecast
                {
                    Date = new DateOnly(2026, 3, 15),
                    TemperatureC = 20,
                    Summary = "Mild"
                }
            };
        }
    }
}
