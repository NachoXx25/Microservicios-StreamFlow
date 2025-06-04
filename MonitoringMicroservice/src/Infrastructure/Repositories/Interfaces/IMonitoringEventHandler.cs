using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringMicroservice.src.Infrastructure.MessageBroker.Models;

namespace MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IMonitoringEventHandler
    {
        Task HandleActionEvent(ActionEvent actionEvent);

        Task HandleErrorEvent(ErrorEvent errorEvent);
    }
}