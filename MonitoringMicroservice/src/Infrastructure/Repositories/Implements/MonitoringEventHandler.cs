using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringMicroservice.src.Infrastructure.Data;
using MonitoringMicroservice.src.Infrastructure.MessageBroker.Models;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace MonitoringMicroservice.src.Infrastructure.Repositories.Implements
{
    public class MonitoringEventHandler : IMonitoringEventHandler
    {
        private readonly MonitoringContext _context;

        public MonitoringEventHandler(MonitoringContext context)
        {
            _context = context;
        }

        public async Task HandleActionEvent(ActionEvent actionEvent)
        {
            try
            {

                Log.Information("Acción recibida: {@ActionEvent}", actionEvent);

                await _context.Actions.AddAsync(new Domain.Models.Action
                {
                    Name = actionEvent.Name,
                    UserId = actionEvent.UserId,
                    UserEmail = actionEvent.UserEmail,
                    MethodUrl = actionEvent.MethodUrl,
                    Timestamp = actionEvent.Timestamp
                });
                await _context.SaveChangesAsync();
                Log.Information("Acción agregada exitosamente: {@ActionEvent}", actionEvent);
           }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de la nueva acción: {@ActionEvent}", actionEvent);
                throw;
            }
        }

        public async Task HandleErrorEvent(ErrorEvent errorEvent)
        {
            try
            {
                Log.Information("Error recibido: {@ErrorEvent}", errorEvent);

                await _context.Errors.AddAsync(new Domain.Models.Error
                {
                    Message = errorEvent.Message,
                    UserId = errorEvent.UserId,
                    UserEmail = errorEvent.UserEmail,
                    Timestamp = errorEvent.Timestamp
                });
                await _context.SaveChangesAsync();
                Log.Information("Error agregado exitosamente: {@ErrorEvent}", errorEvent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al manejar el evento de error: {@ErrorEvent}", errorEvent);
                throw;
            }
        }
    }
}