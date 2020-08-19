using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TempoController: ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        public TempoController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IEnumerable<WeatherForecast> Get(
            [FromServices]IConfiguration config,
            [FromServices]IDistributedCache cache)
        {
            List<WeatherForecast> lista = new List<WeatherForecast>();

            foreach(var item in Summaries)
            {
                string valorJSON = cache.GetString(item);
                var items = JsonSerializer.Deserialize<List<WeatherForecast>>(valorJSON);
                lista.AddRange(items);
            }
            return lista;
        }
    }
}