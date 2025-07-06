using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using RabbitMQ.Client;

namespace SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Services
{
    public class RabbitMQService
    {
        public required ConnectionFactory _connectionFactory { get; set; }

        public required IConnection _connection { get; set; }

        private readonly object _connectionLock = new object();

        private readonly string _hostName;
        private readonly string _userName;
        private readonly string _password;
        private readonly int _port;
        private readonly string _exchangeName;

        public RabbitMQService()
        {
            _hostName = Env.GetBool("IS_LOCAL", true) ? "localhost" : "rabbit_mq";
            _userName = "guest";
            _password = "guest";
            _port = 5672;
            _exchangeName = "StreamFlowExchange";

            CreateConnection();
        }

        public IConnection CreateConnection()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = _hostName,
                UserName = _userName,
                Password = _password,
                Port = _port,
                DispatchConsumersAsync = true,
            };

            lock (_connectionLock)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = _connectionFactory.CreateConnection();
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