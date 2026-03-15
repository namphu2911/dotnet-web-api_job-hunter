using Microsoft.AspNetCore.Mvc;
using JobHunter.Application.Abstractions;
using JobHunter.Application.Contracts;

namespace JobHunter.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IWeatherForecastService _weatherForecastService;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IWeatherForecastService weatherForecastService)
        {
            _logger = logger;
            _weatherForecastService = weatherForecastService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecastDto> Get()
        {
            _logger.LogInformation("Fetching weather forecasts through application service");
            return _weatherForecastService.GetForecasts();
        }
    }
}
