using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using StackExchange.Redis;
using System.Text.Json;
using System.Collections.Generic;

namespace Receive
{
    class Program
    {
        private const string QUENE = "tempo";
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        static void Main(string[] args)
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
                    
                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var lista = JsonSerializer.Deserialize<List<WeatherForecast>>(message);
                        foreach (var item in lista)
                        {
                            SaveRedis(item);
                        }
                    };

                    channel.BasicConsume(queue: QUENE,
                                         autoAck: true,
                                         consumer: consumer);
                }
            }
        }

        private static void SaveRedis(WeatherForecast tempo)
        {
            var rng = new Random();
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("localhost");
            IDatabase conn = muxer.GetDatabase();

            var Summary = Summaries[rng.Next(Summaries.Length)];
            var json = JsonSerializer.Serialize(tempo);

            conn.StringSet(Summary,json);
        }
    }
}
