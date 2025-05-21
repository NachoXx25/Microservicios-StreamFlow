using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using RabbitMQ.Client;
using VideoMicroservice.src.Domain;

namespace VideoMicroservice.Services
{
    public interface IVideoEventService
    {
        Task PublishCreatedVideo(Video video);
        Task PublishUpdatedVideo(Video video);
        Task PublishDeletedVideo(Video video);
    }


    public class VideoEventService : IVideoEventService
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private readonly int _port;
        private readonly string _exchangeName;

        private ConnectionFactory _factory;

        public VideoEventService()
        {
            _hostname = Env.GetString("RABBITMQ_HOST");
            _password = Env.GetString("RABBITMQ_PASSWORD");
            _username = Env.GetString("RABBITMQ_USERNAME");
            _port = Env.GetInt("RABBITMQ_PORT");
            _exchangeName = Env.GetString("RABBITMQ_EXCHANGE");

            _factory = new ConnectionFactory()
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
                
                DeclareAndBindQueue(channel, "video_created_queue", "video.created");
                DeclareAndBindQueue(channel, "video_updated_queue", "video.updated");
                DeclareAndBindQueue(channel, "video_deleted_queue", "video.deleted");
            }
        }

        public Task PublishCreatedVideo(Video video)
        {
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new 
                    {
                        video.Id,
                        video.Title,
                        video.Description,
                        video.Genre,
                        video.IsDeleted,
                        EventType = "VideoCreated"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "video.created",
                        basicProperties: properties,
                        body: body
                    );
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al publicar el evento de video creado", ex);
            }
        }

        public Task PublishDeletedVideo(Video video)
        {
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new
                    {
                        video.Id,
                        video.IsDeleted,
                        EventType = "VideoDeleted"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "video.deleted",
                        basicProperties: properties,
                        body: body
                    );
                    
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al publicar el evento de video borrado", ex);
            }
        }

        public Task PublishUpdatedVideo(Video video)
        {
           try
           {
            using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new
                    {
                        video.Id,
                        video.Title,
                        video.Description,
                        video.Genre,
                        video.IsDeleted,
                        EventType = "VideoUpdated"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";
   
                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "video.updated",
                        basicProperties: properties,
                        body: body
                    );
                }
                return Task.CompletedTask;
           }
           catch (Exception ex)
           {
                throw new Exception("Error al publicar el evento de video modificado", ex);
           }
        }

        private void DeclareAndBindQueue(IModel channel, string queueName, string routingKey)
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
                exchange: _exchangeName,
                routingKey: routingKey
            );
        }

    }
}