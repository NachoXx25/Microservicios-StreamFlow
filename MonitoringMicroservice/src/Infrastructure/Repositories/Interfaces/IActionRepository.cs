using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonitoringMicroservice.src.Domain.Models;

namespace MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IActionRepository
    {
        Task<List<MonitoringMicroservice.src.Domain.Models.Action>> GetAllActions();
    }
}