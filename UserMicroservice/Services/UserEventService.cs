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
    }
    public class UserEventService : IUserEventService
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private readonly int _port;

        
        private ConnectionFactory _factory;
        
        public UserEventService()
        {
            _hostname = Env.GetString("RABBITMQ_HOST") ?? "localhost";
            _username = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
            _password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
            _port = Env.GetInt("RABBITMQ_PORT"); 

            _factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = _port
            };
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

                    string queueName = "user_created_queue";

                    channel.QueueDeclare(
                        queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    var message = new
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        RoleId = user.RoleId,
                        Status = user.Status,
                        CreatedAt = DateTime.UtcNow.ToString("o"),
                        EventType = "UserCreated"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: "",  
                        routingKey: queueName,  
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
    }
}