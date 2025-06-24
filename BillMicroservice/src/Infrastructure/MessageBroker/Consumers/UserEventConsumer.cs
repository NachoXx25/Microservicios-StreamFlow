using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BillMicroservice.src.Domain.Models.User;
using BillMicroservice.src.Infrastructure.MessageBroker.Models;
using BillMicroservice.src.Infrastructure.MessageBroker.Services;
using BillMicroservice.src.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace BillMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class UserEventConsumer : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _exchangeName;

        public required IConnection _connection { get; set; }

        public required IModel _channelCreated { get; set; }

        public required IModel _channelUpdated { get; set; }

        public required IModel _channelDeleted { get; set; }

        public UserEventConsumer(RabbitMQService rabbitMQService, IServiceProvider serviceProvider)
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

                _channelCreated = _connection.CreateModel();
                _channelUpdated = _connection.CreateModel();
                _channelDeleted = _connection.CreateModel();

                _channelCreated.ExchangeDeclare(
                            exchange: _exchangeName,
                            type: _exchangeName,
                            durable: true,
                            autoDelete: false
                );
                _channelUpdated.ExchangeDeclare(
                            exchange: _exchangeName,
                            type: _exchangeName,
                            durable: true,
                            autoDelete: false
                );
                _channelDeleted.ExchangeDeclare(
                            exchange: _exchangeName,
                            type: _exchangeName,
                            durable: true,
                            autoDelete: false
                );

                _channelCreated.BasicQos(0, 1, false);
                _channelCreated.QueueDeclare(
                    "bill_user_created_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelCreated.QueueBind(
                    "bill_user_created_queue",
                    _rabbitMQService.ExchangeName,
                    "user.created"
                );

                _channelUpdated.BasicQos(0, 1, false);
                _channelUpdated.QueueDeclare(
                    "bill_user_updated_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelUpdated.QueueBind(
                    "bill_user_updated_queue",
                    _rabbitMQService.ExchangeName,
                    "user.updated"
                );

                _channelDeleted.BasicQos(0, 1, false);
                _channelDeleted.QueueDeclare(
                    "bill_user_deleted_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelDeleted.QueueBind(
                    "bill_user_deleted_queue",
                    _rabbitMQService.ExchangeName,
                    "user.deleted"
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

            Log.Information("Iniciando el consumidor de eventos de usuario.");

            // User created event consumer
            var consumerCreated = new AsyncEventingBasicConsumer(_channelCreated);
            consumerCreated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var userCreatedEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                    if (userCreatedEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de usuario creado.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IUserEventHandlerRepository>();
                        await userEventHandlerRepository.HandleUserCreated(userCreatedEvent);
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

            // User updated event consumer
            var consumerUpdated = new AsyncEventingBasicConsumer(_channelUpdated);
            consumerUpdated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var userUpdatedEvent = JsonSerializer.Deserialize<UserUpdatedEvent>(message);

                    if (userUpdatedEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de usuario actualizado.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IUserEventHandlerRepository>();
                        await userEventHandlerRepository.HandleUserUpdated(userUpdatedEvent);
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

            // User deleted event consumer
            var consumerDeleted = new AsyncEventingBasicConsumer(_channelDeleted);
            consumerDeleted.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var userDeletedEvent = JsonSerializer.Deserialize<UserDeletedEvent>(message);

                    if (userDeletedEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de usuario eliminado.");
                        return;
                    }
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IUserEventHandlerRepository>();
                        await userEventHandlerRepository.HandleUserDeleted(userDeletedEvent);
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

            // Consumption of user created
            _channelCreated.BasicConsume(
                "bill_user_created_queue",
                false,
                consumerCreated
            );

            // Consumption of user updated
            _channelUpdated.BasicConsume(
                "bill_user_updated_queue",
                false,
                consumerUpdated
            );

            // Consumption of user deleted
            _channelDeleted.BasicConsume(
                "bill_user_deleted_queue",
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