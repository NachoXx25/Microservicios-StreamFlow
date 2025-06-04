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

        private IModel playlistChannel;
        private IModel socialInteractionsChannel;

        private IConnection _connection;

        private ConnectionFactory _factory;

        public VideoEventService()
        {
            _hostname = Env.GetString("RABBITMQ_HOST") ?? "localhost";
            _password = Env.GetString("RABBITMQ_PASSWORD") ?? "guest";
            _username = Env.GetString("RABBITMQ_USERNAME") ?? "guest";
            _port = Env.GetInt("RABBITMQ_PORT");
            _exchangeName = Env.GetString("RABBITMQ_EXCHANGE") ?? "VideoExchange";

            _factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password,
                Port = _port
            };

            // Create a unique connection to RabbitMQ
            _connection = _factory.CreateConnection();

            // Channel for playlist service
            playlistChannel = _connection.CreateModel();
            {
                playlistChannel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: "topic",
                    durable: true,
                    autoDelete: false,
                    arguments: null
                );

                // Queues for playlist consumer
                DeclareAndBindQueue(playlistChannel, "playlist_video_created_queue", "playlist.video.created");
                DeclareAndBindQueue(playlistChannel, "playlist_video_updated_queue", "playlist.video.updated");
                DeclareAndBindQueue(playlistChannel, "playlist_video_deleted_queue", "playlist.video.deleted");
                DeclareAndBindQueue(playlistChannel, "playlist_video_deleted_queue", "playlist.video.deleted");
            }
            
            //Channel for social interactions service
            socialInteractionsChannel = _connection.CreateModel();
            {
                socialInteractionsChannel.ExchangeDeclare(
                    exchange: _exchangeName,
                    type: "topic",
                    durable: true,
                    autoDelete: false,
                    arguments: null
                );

                // Queues for social interactions consumer
                DeclareAndBindQueue(socialInteractionsChannel, "social_interactions_video_created_queue", "social.int.video.created");
                DeclareAndBindQueue(socialInteractionsChannel, "social_interactions_video_updated_queue", "social.int.video.updated");
                DeclareAndBindQueue(socialInteractionsChannel, "social_interactions_video_deleted_queue", "social.int.video.deleted");
            }
        }

        public Task PublishCreatedVideo(Video video)
        {
            try
            {

                var stringId = video.Id.ToString();

                // Publish the event to the playlist service
                var playlistMessage = new 
                {
                    Id = stringId,
                    video.Title,
                    EventType = "VideoCreated"
                };

                var playlistBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(playlistMessage));

                var properties = playlistChannel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                playlistChannel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "playlist.video.created",
                    basicProperties: properties,
                    body: playlistBody
                );

                // Publish the event to the social interactions service
                var socialInteractionsMessage = new 
                {
                    Id = stringId,
                    video.Title,
                    video.Description,
                    video.Genre,
                    video.IsDeleted,
                    EventType = "VideoCreated"
                };

                var socialInteractionsBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(socialInteractionsMessage));

                socialInteractionsChannel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "social.int.video.created",
                    basicProperties: properties,
                    body: socialInteractionsBody
                );
                
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

                var stringId = video.Id.ToString();


                var stringId = video.Id.ToString();

                var message = new
                {
                    Id = stringId,
                    video.IsDeleted,
                    EventType = "VideoDeleted"
                };

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                var properties = playlistChannel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                playlistChannel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "playlist.video.deleted",
                    basicProperties: properties,
                    body: body
                );

                socialInteractionsChannel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "social.int.video.deleted",
                    basicProperties: properties,
                    body: body
                );
                    
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
                var stringId = video.Id.ToString();

                var stringId = video.Id.ToString();

                // Publish the event to the playlist service
                var playlistMessage = new 
                {
                    Id = stringId,
                    video.Title,
                    EventType = "VideoUpdated"
                };

                var playlistBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(playlistMessage));

                var properties = playlistChannel.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";

                playlistChannel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "playlist.video.updated",
                    basicProperties: properties,
                    body: playlistBody
                );

                // Publish the event to the social interactions service
                var socialInteractionsMessage = new 
                {
                    Id = stringId,
                    video.Title,
                    video.Description,
                    video.Genre,
                    EventType = "VideoUpdated"
                };

                var socialInteractionsBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(socialInteractionsMessage));

                socialInteractionsChannel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: "social.int.video.updated",
                    basicProperties: properties,
                    body: socialInteractionsBody
                );

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