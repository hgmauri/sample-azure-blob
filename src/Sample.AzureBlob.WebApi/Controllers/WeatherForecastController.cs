using Microsoft.AspNetCore.Mvc;
using Sample.AzureBlob.Infrastructure.Blob;

namespace Sample.AzureBlob.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IAzureBlobStorage _azureBlobStorage;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAzureBlobStorage azureBlobStorage)
        {
            _logger = logger;
            _azureBlobStorage = azureBlobStorage;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}