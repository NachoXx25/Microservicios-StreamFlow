using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces
{
    public interface IErrorRepository
    {
        Task<List<Domain.Models.Error>> GetAllErrors();
    }
}