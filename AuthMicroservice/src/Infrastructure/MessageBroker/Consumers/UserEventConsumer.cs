using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AuthMicroservice.src.Infrastructure.MessageBroker.Models;
using AuthMicroservice.src.Infrastructure.MessageBroker.Services;
using AuthMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace AuthMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class UserEventConsumer : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IServiceProvider _serviceProvider;
        public required IConnection _connection;
        public required IModel _channelCreated;
        public required IModel _channelUpdated;
        public required IModel _channelDeleted;


        public UserEventConsumer(RabbitMQService rabbitMQService, IServiceProvider serviceProvider)
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
                _channelCreated.ExchangeDeclare(
                            exchange: "StreamFlowExchange",
                            type: ExchangeType.Topic,
                            durable: true,
                            autoDelete: false
                        );
                _channelCreated.BasicQos(0, 1, false);
                _channelCreated.QueueDeclare(
                    queue: "user_created_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _channelCreated.QueueBind(
                    queue: "user_created_queue",
                    exchange: _rabbitMQService.ExchangeName,
                    routingKey: "user.created");

                _channelUpdated = _connection.CreateModel();
                _channelUpdated.ExchangeDeclare(
                    exchange: "StreamFlowExchange",
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                );
                _channelUpdated.BasicQos(0, 1, false);
                _channelUpdated.QueueDeclare(
                    queue: "user_updated_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _channelUpdated.QueueBind(
                    queue: "user_updated_queue",
                    exchange: _rabbitMQService.ExchangeName,
                    routingKey: "user.updated");

                _channelDeleted = _connection.CreateModel();
                _channelDeleted.ExchangeDeclare(
                    exchange: "StreamFlowExchange",
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false
                );
                _channelDeleted.BasicQos(0, 1, false);
                _channelDeleted.QueueDeclare(
                    queue: "user_deleted_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _channelDeleted.QueueBind(
                    queue: "user_deleted_queue",
                    exchange: _rabbitMQService.ExchangeName,
                    routingKey: "user.deleted");
                Log.Information("Canales de RabbitMQ inicializados correctamente.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error initializing RabbitMQ channels.");
                throw;
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            Log.Information("Iniciando el consumidor de eventos de usuario.");

            // Consumiremos la creación de un usuario
            var consumerCreated = new AsyncEventingBasicConsumer(_channelCreated);
            consumerCreated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
                    if (userEvent == null)
                    {
                        Log.Error("Failed to deserialize UserCreatedEvent");
                        return;
                    }
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IUserEventHandlerRepository>();
                        await userEventHandlerRepository.HandleUserCreatedEvent(userEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelCreated.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelCreated.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            // Consumiremos la actualización de un usuario
            var consumerUpdated = new AsyncEventingBasicConsumer(_channelUpdated);
            consumerUpdated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var userEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(message);
                    if (userEvent == null)
                    {
                        Log.Error("Failed to deserialize UserCreatedEvent");
                        return;
                    }
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IUserEventHandlerRepository>();
                        await userEventHandlerRepository.HandleUserUpdatedEvent(userEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelUpdated.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelUpdated.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            //Consumiremos la eliminación de un usuario
            var consumerDeleted = new AsyncEventingBasicConsumer(_channelDeleted);
            consumerDeleted.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var userEvent = JsonSerializer.Deserialize<UserDeletedEvent>(message);
                    if (userEvent == null)
                    {
                        Log.Error("Failed to deserialize UserCreatedEvent");
                        return;
                    }
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IUserEventHandlerRepository>();
                        await userEventHandlerRepository.HandleUserDeletedEvent(userEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelDeleted.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelDeleted.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            _channelCreated.BasicConsume(queue: "user_created_queue", autoAck: false, consumer: consumerCreated);
            _channelUpdated.BasicConsume(queue: "user_updated_queue", autoAck: false, consumer: consumerUpdated);
            _channelDeleted.BasicConsume(queue: "user_deleted_queue", autoAck: false, consumer: consumerDeleted);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    Log.Information("BillEventConsumer: Shutdown requested");
                    break;
                }
            }
            Log.Information("Consumidores de eventos iniciados");

            return;
        }
    }
}