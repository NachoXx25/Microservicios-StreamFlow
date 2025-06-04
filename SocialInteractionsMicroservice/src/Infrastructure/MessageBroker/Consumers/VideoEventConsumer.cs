using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Models;
using SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Services;
using SocialInteractionsMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace SocialInteractionsMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class VideoEventConsumer : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IServiceProvider _serviceProvider;

        public required IConnection _connection { get; set; }

        public required IModel _channelCreated { get; set; }

        public required IModel _channelUpdated { get; set; }

        public required IModel _channelDeleted { get; set; }

        public VideoEventConsumer(RabbitMQService rabbitMQService, IServiceProvider serviceProvider)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                _connection = _rabbitMQService.CreateConnection();

                _channelCreated = _connection.CreateModel();
                _channelUpdated = _connection.CreateModel();
                _channelDeleted = _connection.CreateModel();

                _channelCreated.BasicQos(0, 1, false);
                _channelCreated.QueueDeclare(
                    "social_interactions_video_created_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelCreated.QueueBind(
                    "social_interactions_video_created_queue",
                    _rabbitMQService.ExchangeName,
                    "social.int.video.created"
                );

                _channelUpdated.BasicQos(0, 1, false);
                _channelUpdated.QueueDeclare(
                    "social_interactions_video_updated_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelUpdated.QueueBind(
                    "social_interactions_video_updated_queue",
                    _rabbitMQService.ExchangeName,
                    "social.int.video.updated"
                );

                _channelDeleted.BasicQos(0, 1, false);
                _channelDeleted.QueueDeclare(
                    "social_interactions_video_deleted_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelDeleted.QueueBind(
                    "social_interactions_video_deleted_queue",
                    _rabbitMQService.ExchangeName,
                    "social.int.video.deleted"
                );

                Log.Information("Canales de RabbitMQ inicializados correctamente.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inicializando los canales de RabbitMQ.");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            Log.Information("Iniciando el consumidor de eventos de video.");

            // Video created event consumer
            var consumerCreated = new AsyncEventingBasicConsumer(_channelCreated);
            consumerCreated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var videoCreatedEvent = JsonSerializer.Deserialize<VideoCreatedEvent>(message);

                    if (videoCreatedEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de video creado.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var videoEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IVideoEventHandlerRepository>();
                        await videoEventHandlerRepository.HandleVideoCreatedEventAsync(videoCreatedEvent);
                    }

                    _channelCreated.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelCreated.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            // Video updated event consumer
            var consumerUpdated = new AsyncEventingBasicConsumer(_channelUpdated);
            consumerUpdated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var videoUpdatedEvent = JsonSerializer.Deserialize<VideoUpdatedEvent>(message);

                    if (videoUpdatedEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de video actualizado.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var videoEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IVideoEventHandlerRepository>();
                        await videoEventHandlerRepository.HandleVideoUpdatedEventAsync(videoUpdatedEvent);
                    }

                    _channelUpdated.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelUpdated.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            // Video deleted event consumer
            var consumerDeleted = new AsyncEventingBasicConsumer(_channelDeleted);
            consumerDeleted.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var videoDeletedEvent = JsonSerializer.Deserialize<VideoDeletedEvent>(message);

                    if (videoDeletedEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de video eliminado.");
                        return;
                    }
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var videoEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IVideoEventHandlerRepository>();
                        await videoEventHandlerRepository.HandleVideoDeletedEventAsync(videoDeletedEvent);
                    }

                    _channelDeleted.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelDeleted.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            // Consumption of video created
            _channelCreated.BasicConsume(
                "social_interactions_video_created_queue",
                false,
                consumerCreated
            );

            // Consumption of video updated
            _channelUpdated.BasicConsume(
                "social_interactions_video_updated_queue",
                false,
                consumerUpdated
            );

            // Consumption of video deleted
            _channelDeleted.BasicConsume(
                "social_interactions_video_deleted_queue",
                false,
                consumerDeleted
            );

            Log.Information("Consumidores de eventos iniciados");
            return Task.CompletedTask;
        }
        
        public override void Dispose()
        {
            _channelCreated.Close();
            _channelUpdated.Close();
            _channelDeleted.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}