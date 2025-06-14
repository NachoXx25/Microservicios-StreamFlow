using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using RabbitMQ.Client;

namespace PlaylistMicroservice.src.Infrastructure.MessageBroker.Services
{
    public class RabbitMQService
    {
        public required IConnection _connection;
        public required ConnectionFactory _factory;

        private readonly Object _connectionLock = new object();
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private readonly string _exchangeName;

        public RabbitMQService()
        {
            _hostname = Env.GetString("RABBITMQ_HOST") ?? "localhost";
            _username = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
            _password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
            _port = Env.GetInt("RABBITMQ_PORT");
            _exchangeName = "StreamFlowExchange";

            CreateConnection();
        }

        public IConnection CreateConnection()
        {
            _factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = _port,
                DispatchConsumersAsync = true
            };
            lock (_connectionLock)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    _connection = _factory.CreateConnection();
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