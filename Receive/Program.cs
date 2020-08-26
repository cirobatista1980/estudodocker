using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using StackExchange.Redis;
using System.Text.Json;

namespace Receive
{
    class Program
    {
        private const string queueName = "tempo";
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                Port = 5672,
                UserName = "testes",
                Password = "RabbitMQ2019!"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);


                    var consumer = new EventingBasicConsumer(channel);

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var item = JsonSerializer.Deserialize<WeatherForecast>(message);
                        SaveRedis(item);

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    channel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    
                    Console.ReadLine();
                }
            }
        }

        private static void SaveRedis(WeatherForecast tempo)
        {
            ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect("redis");
            IDatabase conn = muxer.GetDatabase();
            var json = JsonSerializer.Serialize(tempo);

            conn.StringSet(tempo.summary, json);
        }
    }
}
