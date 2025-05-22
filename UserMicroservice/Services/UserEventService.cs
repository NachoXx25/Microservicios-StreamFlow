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
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private readonly int _port;
        private readonly string _exchangeName;

        private ConnectionFactory _factory;
        
        public UserEventService()
        {
            _hostname = Env.GetString("RABBITMQ_HOST") ?? "localhost";
            _username = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
            _password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
            _port = Env.GetInt("RABBITMQ_PORT"); 
            _exchangeName = Env.GetString("RABBITMQ_EXCHANGE") ?? "user_events";
            _factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = _port
            };

            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();
            {
                channel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: "topic", 
                    durable: true,
                    autoDelete: false,
                    arguments: null
                );
                
                DeclareAndBindQueue(channel, "user_created_queue", "user.created");
                DeclareAndBindQueue(channel, "user_updated_queue", "user.updated");
                DeclareAndBindQueue(channel, "user_deleted_queue", "user.deleted");
            }
        }
        
        /// <summary>
        /// Declara y vincula una cola al exchange.
        /// </summary>
        /// <param name="channel">El canal de comunicación con RabbitMQ.</param>
        /// <param name="queueName">El nombre de la cola.</param>
        /// <param name="routingKey">La clave de enrutamiento para el evento.</param>
        private void DeclareAndBindQueue(IModel channel, string queueName, string routingKey)
        {
            // Declara cola
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            
            // Vincula cola al exchange
            channel.QueueBind(
                queue: queueName,
                exchange: _exchangeName,
                routingKey: routingKey
            );
        }
        /// <summary>
        /// Publica un evento de usuario creado en RabbitMQ.
        /// </summary>
        /// <param name="user">El usuario que se ha creado.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
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

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "user.created",
                        basicProperties: properties,
                        body: body
                    );
                    
                    Log.Information($"Evento publicado exitosamente para usuario: {user.Email}");
                }
                return Task.CompletedTask;
            }
            catch(Exception ex)
            {
                Log.Error(ex, $"Error al publicar evento de usuario creado para {user.Email}");
                throw new Exception("Error al publicar el evento de usuario creado", ex);
            }
        }

        /// <summary>
        /// Publica un evento de usuario eliminado en RabbitMQ.
        /// </summary>
        /// <param name="user">El usuario que se ha eliminado.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        public Task PublishUserDeletedEvent(User user)
        {
            try{
                Log.Information($"Publicando evento de usuario eliminado - Email: {user.Email}, Nombre: {user.FirstName} {user.LastName}");
                
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new
                    {
                        Id = user.Id,
                        Status = user.Status,
                        UpdatedAt = DateTime.UtcNow.ToString("o"),
                        EventType = "UserDeleted"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "user.deleted",
                        basicProperties: properties,
                        body: body
                    );
                    
                    Log.Information($"Evento publicado exitosamente para usuario eliminado: {user.Email}");
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al publicar evento de usuario eliminado para {user.Email}");
                throw new Exception("Error al publicar el evento de usuario eliminado", ex);
            }
        }

        /// <summary>
        /// Publica un evento de usuario actualizado en RabbitMQ.
        /// </summary>
        /// <param name="user">El usuario que se ha actualizado.</param>
        /// <returns>Tarea que representa la operación asincrónica.</returns>
        public Task PublishUserUpdatedEvent(User user)
        {
            try
            {
                Log.Information($"Publicando evento de usuario actualizado - Email: {user.Email}, Nombre: {user.FirstName} {user.LastName}");
                
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
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

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "user.updated",
                        basicProperties: properties,
                        body: body
                    );
                    
                    Log.Information($"Evento publicado exitosamente para usuario: {user.Email}");
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error al publicar evento de usuario actualizado para {user.Email}");
                throw new Exception("Error al publicar el evento de usuario actualizado", ex);
            }
        }
    }
}