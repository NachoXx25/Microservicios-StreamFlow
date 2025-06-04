using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringMicroservice.src.Application.DTOs;

namespace MonitoringMicroservice.src.Application.Services.Interfaces
{
    public interface IMonitoringService
    {

        Task<List<ActionDTO>> GetAllActions();

        Task<List<ErrorDTO>> GetAllErrors();
    }
}