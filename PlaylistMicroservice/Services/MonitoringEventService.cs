using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DotNetEnv;
using PlaylistMicroservice.src.Domain.Models;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Services;
using RabbitMQ.Client;
using Serilog;

namespace PlaylistMicroservice.Services
{
    public interface IMonitoringEventService
    {
        Task PublishActionEventAsync(ActionEvent actionEvent);
        Task PublishErrorEventAsync(ErrorEvent errorEvent);
    }
    public class MonitoringEventService : IMonitoringEventService
    {
        private readonly string _exchangeName;
        private readonly IModel _errorChannel;
        private readonly IModel _actionChannel;

        private readonly RabbitMQService _rabbitMQService;

        public MonitoringEventService()
        {


            _exchangeName = "StreamFlowExchange";

            var factory = new ConnectionFactory
            {
                HostName = Env.GetBool("IS_LOCAL", true) ? "localhost" : "rabbit_mq",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };
            _rabbitMQService = new RabbitMQService
            {
                _factory = factory,
                _connection = factory.CreateConnection()
            };

            try
            {
                _errorChannel = _rabbitMQService._connection.CreateModel();
                _actionChannel = _rabbitMQService._connection.CreateModel();

                SetupErrorExchange();
                SetupActionExchange();

                Log.Information("UserEventService inicializado con conexiones persistentes");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inicializando UserEventService");
                throw;
            }
        }

        private void SetupErrorExchange()
        {
            _errorChannel.ExchangeDeclare(
                exchange: _exchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_errorChannel, "Error_queue", "error.generated", _exchangeName);
        }

        private void SetupActionExchange()
        {
            _actionChannel.ExchangeDeclare(
                exchange: _exchangeName,
                type: "topic",
                durable: true,
                autoDelete: false,
                arguments: null
            );

            DeclareAndBindQueue(_actionChannel, "Action_queue", "action.generated", _exchangeName);
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
            var properties = _errorChannel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return properties;
        }
        public Task PublishActionEventAsync(ActionEvent actionEvent)
        {
            try
            {
                Log.Information("Publicando evento de acción: {ActionMessage} en servicio: {Service}", actionEvent.ActionMessage, actionEvent.Service);
                var message = new
                {
                    actionEvent.ActionMessage,
                    actionEvent.Service,
                    actionEvent.UserId,
                    actionEvent.UserEmail,
                    actionEvent.UrlMethod,
                    Timestamp = DateTime.UtcNow
                };
                var body = JsonSerializer.SerializeToUtf8Bytes(message);
                var properties = CreateBasicProperties();
                PublishToExchange(_actionChannel, _exchangeName, "action.generated", body, properties);

                Log.Information("Evento de acción publicado correctamente");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al publicar evento de acción");
                throw;
            }
        }

        public Task PublishErrorEventAsync(ErrorEvent errorEvent)
        {
            try
            {
                Log.Information("Publicando evento de error: {ErrorMessage} en servicio: {Service}", errorEvent.ErrorMessage, errorEvent.Service);
                var message = new
                {
                    errorEvent.ErrorMessage,
                    errorEvent.Service,
                    errorEvent.UserId,
                    errorEvent.UserEmail,
                    Timestamp = DateTime.UtcNow
                };
                var body = JsonSerializer.SerializeToUtf8Bytes(message);
                var properties = CreateBasicProperties();
                PublishToExchange(_errorChannel, _exchangeName, "error.generated", body, properties);

                Log.Information("Evento de error publicado correctamente");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al publicar evento de error");
                throw;
            }
        }
    }
}