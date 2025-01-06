using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using MessageQueue.Models;

namespace MessageQueue
{
    public class RabbitMqConsumer
    {
        private readonly string _hostname = "localhost";
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMqConsumer()
        {
            InitializeConsumer();
        }

        private async Task InitializeConsumer()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.QueueDeclareAsync(queue: "tSystem", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += OnMessageReceived;
            await _channel.BasicConsumeAsync(queue: "tSystem", autoAck: false, consumer: consumer);

        }

        private async Task OnMessageReceived(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                var notificationMessage = JsonConvert.DeserializeObject<NotificationMessage>(message);
                ProcessMessage(notificationMessage);

                //_channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                //_channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        }

        private async Task ProcessMessage(NotificationMessage message)
        {
            var emailClient = new ElasticEmailClient("api-key");
            var result = await emailClient.SendEmailAsync("my-mail", "MEssage Queue", message.Content);
            Console.WriteLine(result);
            Console.WriteLine("Received message: " + message.Content);
        }
    }
}