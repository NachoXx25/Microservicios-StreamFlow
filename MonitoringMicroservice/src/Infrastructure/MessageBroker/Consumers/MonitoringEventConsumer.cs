using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringMicroservice.src.Infrastructure.MessageBroker.Models;
using MonitoringMicroservice.src.Infrastructure.MessageBroker.Services;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace MonitoringMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class MonitoringEventConsumer : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IServiceProvider _serviceProvider;

        public required IConnection _connection { get; set; }

        public required IModel _channelAction { get; set; }

        public required IModel _channelError { get; set; }

        public MonitoringEventConsumer(RabbitMQService rabbitMQService, IServiceProvider serviceProvider)
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

                _channelAction = _connection.CreateModel();
                _channelError = _connection.CreateModel();

                _channelAction.BasicQos(0, 1, false);
                _channelAction.QueueDeclare(
                    "Action_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelAction.QueueBind(
                    "Action_queue",
                    _rabbitMQService.ExchangeName,
                    "action.generated"
                );

                _channelError.BasicQos(0, 1, false);
                _channelError.QueueDeclare(
                    "Error_queue",
                    true,
                    false,
                    false,
                    null
                );
                _channelError.QueueBind(
                    "Error_queue",
                    _rabbitMQService.ExchangeName,
                    "error.generated"
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

            Log.Information("Iniciando el consumidor de eventos de acciones y errores.");

            // Action event consumer
            var actionConsumer = new AsyncEventingBasicConsumer(_channelAction);
            actionConsumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var actionEvent = JsonSerializer.Deserialize<ActionEvent>(message);

                    if (actionEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de acción.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var monitoringEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IMonitoringEventHandler>();
                        await monitoringEventHandlerRepository.HandleActionEvent(actionEvent);
                    }

                    _channelAction.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelAction.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            // error event consumer
            var errorConsumer = new AsyncEventingBasicConsumer(_channelError);
            errorConsumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information("Mensaje recibido: {Message}", message);
                    var errorEvent = JsonSerializer.Deserialize<ErrorEvent>(message);

                    if (errorEvent == null)
                    {
                        Log.Error("Falló la deserialización del evento de error.");
                        return;
                    }

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var monitoringEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IMonitoringEventHandler>();
                        await monitoringEventHandlerRepository.HandleErrorEvent(errorEvent);
                    }

                    _channelError.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al recibir el mensaje de RabbitMQ.");
                    bool requeue = ex is DbUpdateException || ex is TimeoutException;
                    _channelError.BasicNack(ea.DeliveryTag, false, requeue);
                }
            };

            // Consumption of action
            _channelAction.BasicConsume(
                "Action_queue",
                false,
                actionConsumer
            );

            // Consumption of error
            _channelError.BasicConsume(
                "Error_queue",
                false,
                errorConsumer
            );

            Log.Information("Consumidores de eventos iniciados");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channelAction.Close();
            _channelError.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}