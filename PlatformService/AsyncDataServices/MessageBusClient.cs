using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _config;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        public MessageBusClient(IConfiguration config)
        {
            this._config = config;
            var factory = new ConnectionFactory() { HostName = _config["RabbitMQHost"], 
            Port = int.Parse(_config["RabbitMQPort"])};

            try 
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
                Console.WriteLine("--> connected to message bus");

            } catch(Exception ex) {
                Console.WriteLine($"==> Could not connect to message bus {ex.Message}");
            }
        }
        public void PublishNewPlatform(PlatformPublishedDto platfomrPublishDto)
        {
            var message = JsonSerializer.Serialize(platfomrPublishDto);
            
            if(_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connectino Open, sending message....");
                //Send message below
                SendMessage(message);

            } else {
                Console.WriteLine("--->  RabbitMq Connections closed, not sending..");
            }
        }

        private void SendMessage(string message) {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: "trigger", 
                routingKey:"", 
                basicProperties: null,
                body: body);

            Console.WriteLine($"--> Message sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("Messagebus Disposed");
            if(_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("---> RabbitMQ conneciton shutdown");
        }
    }
}