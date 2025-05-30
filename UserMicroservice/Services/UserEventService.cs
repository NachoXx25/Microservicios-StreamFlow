using System;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using RabbitMQ.Client;
using UserMicroservice.src.Domain;
using Serilog;
using DotNetEnv;

namespace UserMicroservice.Services
{
    public interface IUserEventService
    {
        Task PublishUserCreatedEvent(User user);
        Task PublishUserUpdatedEvent(User user);
        Task PublishUserDeletedEvent(User user);
    }
    
    public class UserEventService : IUserEventService
    {
        private readonly string _userExchangeName;
        private readonly string _billExchangeName;
        private readonly IConnection _connection;
        private readonly IModel _userChannel;
        private readonly IModel _billChannel;
        private bool _disposed = false;

        public UserEventService()
        {
            var hostname = Env.GetString("RABBITMQ_HOST") ?? "localhost";
            var username = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
            var password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
            var port = Env.GetInt("RABBITMQ_PORT", 5672);

            _userExchangeName = Env.GetString("RABBITMQ_EXCHANGE") ?? "user_events";
            _billExchangeName = "BillExchange";

            var factory = new ConnectionFactory
            {
                HostName = hostname,
                UserName = username,
                Password = password,
                Port = port
            };

            try
            {
                _connection = factory.CreateConnection();

                _userChannel = _connection.CreateModel();
                _billChannel = _connection.CreateModel();

                SetupUserExchange();
                SetupBillExchange();

                Log.Information("UserEventService inicializado con conexiones persistentes");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inicializando UserEventService");
                throw;
            }
        }

        private void SetupUserExchange()
        {
            _userChannel.ExchangeDeclare(
                exchange: _userExchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_userChannel, "user_created_queue", "user.created", _userExchangeName);
            DeclareAndBindQueue(_userChannel, "user_updated_queue", "user.updated", _userExchangeName);
            DeclareAndBindQueue(_userChannel, "user_deleted_queue", "user.deleted", _userExchangeName);
        }

        private void SetupBillExchange()
        {

            _billChannel.ExchangeDeclare(
                exchange: _billExchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_billChannel, "bill_user_created_queue", "user.created", _billExchangeName);
            DeclareAndBindQueue(_billChannel, "bill_user_updated_queue", "user.updated", _billExchangeName);
            DeclareAndBindQueue(_billChannel, "bill_user_deleted_queue", "user.deleted", _billExchangeName);
        }

        private void DeclareAndBindQueue(IModel channel, string queueName, string routingKey, string exchangeName)
        {
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey
            );
        }

        public Task PublishUserCreatedEvent(User user)
        {
            try
            {
                Log.Information($"Publicando evento de usuario creado - Email: {user.Email}, Nombre: {user.FirstName} {user.LastName}");
                
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new 
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        RoleId = user.RoleId,
                        Status = user.Status,
                        PasswordHash = user.PasswordHash,
                        CreatedAt = DateTime.UtcNow.ToString("o"),
                        EventType = "UserCreated"
                    };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var properties = CreateBasicProperties();

                PublishToExchange(_userChannel, _userExchangeName, "user.created", body, properties);
                PublishToExchange(_billChannel, _billExchangeName, "user.created", body, properties);

                Log.Information("Evento UserCreated publicado en ambos exchanges para: {Email}", user.Email);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error publicando evento UserCreated para {Email}", user.Email);
                throw new Exception("Error al publicar el evento de usuario creado", ex);
            }
        }

        public Task PublishUserUpdatedEvent(User user)
        {
            try
            {
                Log.Information("Publicando evento UserUpdated - Email: {Email}, Nombre: {FirstName} {LastName}",
                               user.Email, user.FirstName, user.LastName);

                var message = new
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UpdatedAt = DateTime.UtcNow.ToString("o"),
                    EventType = "UserUpdated"
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var properties = CreateBasicProperties();

                PublishToExchange(_userChannel, _userExchangeName, "user.updated", body, properties);
                PublishToExchange(_billChannel, _billExchangeName, "user.updated", body, properties);

                Log.Information("Evento UserUpdated publicado en ambos exchanges para: {Email}", user.Email);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error publicando evento UserUpdated para {Email}", user.Email);
                throw new Exception("Error al publicar el evento de usuario actualizado", ex);
            }
        }

        public Task PublishUserDeletedEvent(User user)
        {
            try
            {
                Log.Information("Publicando evento UserDeleted - Email: {Email}, Nombre: {FirstName} {LastName}",
                               user.Email, user.FirstName, user.LastName);

                var message = new
                {
                    Id = user.Id,
                    Status = user.Status,
                    UpdatedAt = DateTime.UtcNow.ToString("o"),
                    EventType = "UserDeleted"
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
                var properties = CreateBasicProperties();

                PublishToExchange(_userChannel, _userExchangeName, "user.deleted", body, properties);
                PublishToExchange(_billChannel, _billExchangeName, "user.deleted", body, properties);

                Log.Information("Evento UserDeleted publicado en ambos exchanges para: {Email}", user.Email);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error publicando evento UserDeleted para {Email}", user.Email);
                throw new Exception("Error al publicar el evento de usuario eliminado", ex);
            }
        }

        private void PublishToExchange(IModel channel, string exchangeName, string routingKey, byte[] body, IBasicProperties properties)
        {
            try
            {
                channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body
                );

                Log.Debug("Mensaje enviado a exchange: {Exchange}, routing key: {RoutingKey}", exchangeName, routingKey);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error publicando en exchange: {Exchange}", exchangeName);
                throw;
            }
        }

        private IBasicProperties CreateBasicProperties()
        {
            var properties = _userChannel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return properties;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    _userChannel?.Close();
                    _billChannel?.Close();
                    _connection?.Close();

                    _userChannel?.Dispose();
                    _billChannel?.Dispose();
                    _connection?.Dispose();

                    Log.Information("Conexiones RabbitMQ cerradas correctamente");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error cerrando conexiones RabbitMQ");
                }

                _disposed = true;
            }
        }

        ~UserEventService()
        {
            Dispose(false);
        }
    }
}