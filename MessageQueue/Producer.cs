using MessageQueue.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace MessageQueue
{
    public class RabbitMqProducer
    {
        private readonly string _hostname = "localhost";
        private IConnection _connection;
        private IChannel _channel;


        public RabbitMqProducer() { }

        public async Task InitQueue()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var conntection = await factory.CreateConnectionAsync();
            if (conntection != null)
            {
                _connection = conntection!;
                var channel = await conntection.CreateChannelAsync();
                _channel = channel;
            }
        }

        public async Task SendMessage(NotificationMessage message)
        {
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);


            // Создание свойств сообщения
            var properties = new BasicProperties();
            properties.Persistent = true; // Сделать сообщение постоянным
            if (_connection == null || _channel == null) { await InitQueue(); }

            _channel.BasicPublishAsync(
                exchange: "",
                routingKey: "tSystem",
                mandatory: true,
                basicProperties: properties,
                body: new ReadOnlyMemory<byte>(body));
        }
    }
}