using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BillMicroservice.Protos;
using BillMicroservice.src.Application.DTOs;
using BillMicroservice.src.Domain.Models.Bill;
using DotNetEnv;
using RabbitMQ.Client;

namespace BillMicroservice.Services
{
    public interface IBillEventService
    {
        Task PublishUpdatedBillEvent(UpdatedBillDTO updatedBill);
    }

    public class BillEventService : IBillEventService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private readonly string _exchangeName;

        private ConnectionFactory _factory;

        public BillEventService()
        {
            _hostname = "localhost";
            _username = "guest";
            _password = "guest";
            _port = 5672;
            _exchangeName = "StreamFlowExchange";

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

                DeclareAndBindQueue(channel, "bill_updated_queue", "bill.updated");
            }
        }

        public Task PublishUpdatedBillEvent(UpdatedBillDTO updatedBill)
        {
            try
            {
                using (var connection = _factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {

                    var userName = updatedBill.FirstName + " " + updatedBill.LastName; 

                    var message = new
                    {
                        BillId = updatedBill.Id.ToString(),
                        UserEmail = updatedBill.UserEmail,
                        UserName = userName,
                        BillStatus = updatedBill.StatusName,
                        BillAmount = updatedBill.Amount.ToString(),
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.ContentType = "application/json";

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: "bill.updated",
                        basicProperties: properties,
                        body: body
                    );
                }
                
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al publicar el evento de factura modificada", ex);
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