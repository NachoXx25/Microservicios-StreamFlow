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
        private readonly IConnection _connection;
        private readonly IModel _Channel;
        private bool _disposed = false;

        public UserEventService()
        {
            var hostname = Env.GetBool("IS_LOCAL", true) ? "localhost" : "rabbit_mq";
            var username = "guest";
            var password = "guest";
            var port = 5672;

            _userExchangeName = "StreamFlowExchange";

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

                _Channel = _connection.CreateModel();
                _Channel = _connection.CreateModel();
                _Channel = _connection.CreateModel();

                SetupUserExchange();
                SetupBillExchange();
                SetupPlaylistExchange();

                Log.Information("UserEventService inicializado con conexiones persistentes");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inicializando UserEventService");
                throw;
            }
        }

        private void SetupPlaylistExchange()
        {
            _Channel.ExchangeDeclare(
                exchange: _userExchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_Channel, "playlist_user_created_queue", "user.created", _userExchangeName);
            DeclareAndBindQueue(_Channel, "playlist_user_updated_queue", "user.updated", _userExchangeName);
        }

        private void SetupUserExchange()
        {
            _Channel.ExchangeDeclare(
                exchange: _userExchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_Channel, "user_created_queue", "user.created", _userExchangeName);
            DeclareAndBindQueue(_Channel, "user_updated_queue", "user.updated", _userExchangeName);
            DeclareAndBindQueue(_Channel, "user_deleted_queue", "user.deleted", _userExchangeName);
        }

        private void SetupBillExchange()
        {

            _Channel.ExchangeDeclare(
                exchange: _userExchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_Channel, "bill_user_created_queue", "user.created", _userExchangeName);
            DeclareAndBindQueue(_Channel, "bill_user_updated_queue", "user.updated", _userExchangeName);
            DeclareAndBindQueue(_Channel, "bill_user_deleted_queue", "user.deleted", _userExchangeName);
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

                PublishToExchange(_Channel, _userExchangeName, "user.created", body, properties);

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

                PublishToExchange(_Channel, _userExchangeName, "user.updated", body, properties);

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

                PublishToExchange(_Channel, _userExchangeName, "user.deleted", body, properties);

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
            var properties = _Channel.CreateBasicProperties();
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
                    _Channel?.Close();
                    _connection?.Close();

                    _Channel?.Dispose();
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