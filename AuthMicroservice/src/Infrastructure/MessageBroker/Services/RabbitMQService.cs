using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using RabbitMQ.Client;

namespace AuthMicroservice.src.Infrastructure.MessageBroker.Services
{
    public class RabbitMQService
    {
        public required ConnectionFactory _Factory;
        public required IConnection _connection;
        private readonly object _connectionLock = new object();

        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private readonly string _exchangeName;

        public RabbitMQService()
        {

            _hostname = Env.GetBool("IS_LOCAL", true) ? "localhost" : "rabbit_mq";
            _username = "guest";
            _password = "guest";
            _port = 5672;
            _exchangeName = "StreamFlowExchange";

            CreateConnection();
        }
        public IConnection CreateConnection()
        {
            _Factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = _port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(15),
                UseBackgroundThreadsForIO = true
            };

            lock (_connectionLock)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = _Factory.CreateConnection();
                }
            }
            return _connection;
        }

        public string ExchangeName => _exchangeName;
        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}