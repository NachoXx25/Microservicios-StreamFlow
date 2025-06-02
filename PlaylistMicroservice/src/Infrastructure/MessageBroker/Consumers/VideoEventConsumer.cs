using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Models;
using PlaylistMicroservice.src.Infrastructure.MessageBroker.Services;
using PlaylistMicroservice.src.Infrastructure.Repositories.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace PlaylistMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class VideoEventConsumer : BackgroundService

    {

        private readonly RabbitMQService _rabbitMQService;

        private readonly IServiceProvider _provider;
        public required IConnection _connection;

        public required IModel _channelCreated;
        public required IModel _channelUpdated;
        public required IModel _channelDeleted;

        public VideoEventConsumer(RabbitMQService rabbitMQService, IServiceProvider provider)
        {
            _rabbitMQService = rabbitMQService;
            _provider = provider;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                _connection = _rabbitMQService.CreateConnection();
                _channelCreated = _connection.CreateModel();
                _channelCreated.BasicQos(0, 1, false);
                _channelCreated.QueueDeclare(
                    queue: "playlist_video_created_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                _channelCreated.QueueBind(
                    queue: "playlist_video_created_queue",
                    exchange: "VideoExchange",
                    routingKey: "playlist.video.created"
                );

                _channelUpdated = _connection.CreateModel();
                _channelUpdated.BasicQos(0, 1, false);
                _channelUpdated.QueueDeclare(
                    queue: "playlist_video_updated_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                _channelUpdated.QueueBind(
                    queue: "playlist_video_updated_queue",
                    exchange: "VideoExchange",
                    routingKey: "playlist.video.updated"
                );

                _channelDeleted = _connection.CreateModel();
                _channelDeleted.BasicQos(0, 1, false);
                _channelDeleted.QueueDeclare(
                    queue: "playlist_video_deleted_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                _channelDeleted.QueueBind(
                    queue: "playlist_video_deleted_queue",
                    exchange: "VideoExchange",
                    routingKey: "playlist.video.deleted"
                );
            }
            catch (Exception ex)
            {
                Log.Error("Error al iniciar la conexión con Rabbit MQ", ex.Message);
                throw;
            }
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            Log.Information("Iniciando el consumidor de eventos de video.");

            var consumerCreated = new AsyncEventingBasicConsumer(_channelCreated);
            consumerCreated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);

                    var videoEvent = JsonSerializer.Deserialize<VideoCreated>(message);
                    if (videoEvent == null)
                    {
                        Log.Error("Failed to deserialize UserCreatedEvent");
                        return;
                    }
                    using (var scope = _provider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IVideoEventHandlerRepository>();
                        await userEventHandlerRepository.HandleVideoCreatedEvent(videoEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelCreated.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error("Error en el servicio consumidor {ex.Message}", ex.Message);
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelCreated.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            var consumerUpdated = new AsyncEventingBasicConsumer(_channelUpdated);
            consumerUpdated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);

                    var videoEvent = JsonSerializer.Deserialize<VideoUpdated>(message);
                    if (videoEvent == null)
                    {
                        Log.Error("Failed to deserialize UserCreatedEvent");
                        return;
                    }
                    using (var scope = _provider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IVideoEventHandlerRepository>();
                        await userEventHandlerRepository.HandleVideoUpdatedEvent(videoEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelUpdated.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error("Error en el servicio consumidor {ex.Message}", ex.Message);
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelUpdated.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            var consumerDeleted = new AsyncEventingBasicConsumer(_channelDeleted);
            consumerDeleted.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);

                    var videoEvent = JsonSerializer.Deserialize<VideoDeleted>(message);
                    if (videoEvent == null)
                    {
                        Log.Error("Failed to deserialize UserCreatedEvent");
                        return;
                    }
                    using (var scope = _provider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IVideoEventHandlerRepository>();
                        await userEventHandlerRepository.HandleVideoDeletedEvent(videoEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelDeleted.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error("Error en el servicio consumidor {ex.Message}", ex.Message);
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelDeleted.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            _channelCreated.BasicConsume(queue: "playlist_video_created_queue", autoAck: false, consumer: consumerCreated);
            _channelUpdated.BasicConsume(queue: "playlist_video_updated_queue", autoAck: false, consumer: consumerUpdated);
            _channelDeleted.BasicConsume(queue: "playlist_video_deleted_queue", autoAck: false, consumer: consumerDeleted);

            Log.Information("Consumidores de eventos iniciados");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channelCreated?.Close();
            _channelUpdated?.Close();
            _channelDeleted?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}