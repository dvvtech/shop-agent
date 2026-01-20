using Microsoft.AspNetCore.Mvc;
using ShopAgent.Api.BLL.Abstract;

namespace ShopAgent.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly IAiClient _aiClient;

    private readonly ILogger<WeatherForecastController> _logger;


    public WeatherForecastController(
        IAiClient aiClient,
        ILogger<WeatherForecastController> logger)
    {
        _aiClient = aiClient;
        _logger = logger;
    }

    [HttpGet("test")]
    public async Task<string> Test()
    {
        var res = await _aiClient.GetTextResponseAsync("напиши 4 стишье", "ты писатель");
        return "123";
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
