using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;

namespace Send
{
    public class Program
    {
        private const string QUENE = "tempo";
        private const string ROUTINGKEY = "tempo";
        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: QUENE,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    var msgs = await GetMessage();
                    
                    foreach(var item in msgs)
                    {
                        var message = JsonSerializer.Serialize<WeatherForecast>(item);
                        var body = Encoding.UTF8.GetBytes(message);

                        channel.BasicPublish(exchange: "",
                                         routingKey: ROUTINGKEY,
                                         basicProperties: null,
                                         body: body);
                    }
                }
            }
        }

        private static async Task<List<WeatherForecast>> GetMessage()
        {
            var streamTask = client.GetStreamAsync("https://localhost:5001/WeatherForecast/");
            var models = await JsonSerializer.DeserializeAsync<List<WeatherForecast>>(await streamTask);
            return models;
        }
    }
}
