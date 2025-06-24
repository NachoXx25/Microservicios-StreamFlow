using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using VideoMicroservice.src.Infrastructure.MessageBroker.Models;
using VideoMicroservice.src.Infrastructure.MessageBroker.Services;
using VideoMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace VideoMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class SocialInteractionEventConsumer : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _exchangeName;

        public required IConnection _connection { get; set; }

        public required IModel _channelLiked { get; set; }

        public SocialInteractionEventConsumer(RabbitMQService rabbitMQService, IServiceProvider serviceProvider)
        {
            _rabbitMQService = rabbitMQService;
            _serviceProvider = serviceProvider;
            _exchangeName = _rabbitMQService.ExchangeName;
            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                _connection = _rabbitMQService.CreateConnection();

                _channelLiked = _connection.CreateModel();

                _channelLiked.ExchangeDeclare(
                            exchange: _exchangeName,
                            type: _exchangeName,
                            durable: true,
                            autoDelete: false
                );

                _channelLiked.BasicQos(0, 1, false);
                _channelLiked.QueueDeclare(
                    "social_interactions_like_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelLiked.QueueBind(
                    "social_interactions_like_queue",
                    _rabbitMQService.ExchangeName,
                    "social.interactions.like"
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

            // Liked video event consumer
            var consumerLiked = new AsyncEventingBasicConsumer(_channelLiked);
            consumerLiked.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var likeEvent = JsonSerializer.Deserialize<LikeEvent>(message);

                    if (likeEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de like asignado.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var socialInteractionEventHandlerRepository = scope.ServiceProvider.GetRequiredService<ISocialInteractionEventHandlerRepository>();
                        await socialInteractionEventHandlerRepository.HandleLikedVideoEvent(likeEvent);
                    }

                    _channelLiked.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelLiked.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            // Consumption of liked video event
            _channelLiked.BasicConsume(
                "social_interactions_like_queue",
                false,
                consumerLiked
            );

            Log.Information("Consumidores de eventos iniciados");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channelLiked.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}