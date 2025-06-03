using EmailMicroservice.src.Infrastructure.MessageBroker.Models;
using EmailMicroservice.src.Infrastructure.MessageBroker.Services;
using EmailMicroservice.src.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace EmailMicroservice.src.Infrastructure.MessageBroker.Consumers
{
    public class BillEventConsumer : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private IServiceProvider _provider;
        public required IModel _channelBillUpdated;
        public BillEventConsumer(RabbitMQService rabbitMQService, IServiceProvider provider)
        {
            _rabbitMQService = rabbitMQService;
            _provider = provider;
            InitializeRabbitMQ();
        }

        public void InitializeRabbitMQ()
        {
            try
            {
                _channelBillUpdated = _rabbitMQService.CreateConnection().CreateModel();
                _channelBillUpdated.BasicQos(0, 1, false);
                _channelBillUpdated.QueueDeclare(
                    queue: "bill_updated_queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                _channelBillUpdated.QueueBind(
                    queue: "bill_updated_queue",
                    exchange: _rabbitMQService.ExchangeName,
                    routingKey: "bill.updated"
                    );
            }catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar la conexión con Rabbit MQ: {ex.Message}");
                throw;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            Log.Information("Empezando el consumidor de eventos de facturas 💸");

            var consumerUpdated = new AsyncEventingBasicConsumer(_channelBillUpdated);
            consumerUpdated.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = System.Text.Encoding.UTF8.GetString(body);
                    Log.Information($"Mensaje recibido: {message}");

                    var billEvent = System.Text.Json.JsonSerializer.Deserialize<BillUpdated>(message);
                    if (billEvent == null)
                    {
                        Log.Error("Failed to deserialize BillUpdatedEvent");
                        return;
                    }
                    using (var scope = _provider.CreateScope())
                    {
                        var userEventHandlerRepository = scope.ServiceProvider.GetRequiredService<IBillEventHandler>();
                        await userEventHandlerRepository.HandleBillUpdatedEvent(billEvent);
                    }
                    // Confirmamos el mensaje después de procesarlo
                    _channelBillUpdated.BasicAck(ea.DeliveryTag, false);
                    }catch (Exception ex)
                {
                    Log.Error($"Error al procesar el mensaje: {ex.Message}");
                    // Si ocurre un error, no confirmamos el mensaje para que pueda ser reintentado
                    _channelBillUpdated.BasicNack(ea.DeliveryTag, false, true);
                    return;
                }
            };

            _channelBillUpdated.BasicConsume(
                queue: "bill_updated_queue",
                autoAck: false,
                consumer: consumerUpdated
            );
            Log.Information("Consumidores de eventos iniciados");
            return Task.CompletedTask;
        }
    }
}