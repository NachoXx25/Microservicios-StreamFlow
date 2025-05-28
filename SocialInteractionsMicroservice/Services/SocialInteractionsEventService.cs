using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using RabbitMQ.Client;

namespace SocialInteractionsMicroservice.Services
{
    public interface ISocialInteractionsEventService
    {
        Task PublishLikeEvent(string videoId);
        Task PublishCommentEvent(string videoId, string comment);
    }

    public class SocialInteractionsEventService : ISocialInteractionsEventService
    {

        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private readonly string _exchangeName;

        private ConnectionFactory _factory;

        public SocialInteractionsEventService()
        {
            _hostname = Env.GetString("RABBITMQ_HOST");
            _username = Env.GetString("RABBITMQ_USERNAME");
            _password = Env.GetString("RABBITMQ_PASSWORD");
            _port = Env.GetInt("RABBITMQ_PORT");
            _exchangeName = Env.GetString("RABBITMQ_EXCHANGE_NAME");

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

                DeclareAndBindQueue(channel, "social_interactions_like_queue", "social_interactions.like");
                DeclareAndBindQueue(channel, "social_interactions_comment_queue", "social_interactions.comment");
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

        public Task PublishCommentEvent(string videoId, string comment)
        {
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new
                    {
                        VideoId = videoId,
                        Comment = comment,
                        EventType = "SocialInteractionsComment"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "social_interactions.comment",
                        basicProperties: properties,
                        body: body
                    );
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al publicar el evento de comentario agregado", ex);
            }
        }

        public Task PublishLikeEvent(string videoId)
        {
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var message = new
                    {
                        VideoId = videoId,
                        EventType = "SocialInteractionsLike"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "social_interactions.like",
                        basicProperties: properties,
                        body: body
                    );
                }
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {

                throw new Exception("Error al publicar el evento de like agregado", ex);
            }
        }
    }
}