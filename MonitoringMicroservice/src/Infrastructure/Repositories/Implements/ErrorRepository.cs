using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringMicroservice.src.Infrastructure.Data;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace MonitoringMicroservice.src.Infrastructure.Repositories.Implements
{
    public class ErrorRepository : IErrorRepository
    {
        private readonly MonitoringContext _context;
        public ErrorRepository(MonitoringContext context)
        {
            _context = context;
        }

        public async Task<List<Domain.Models.Error>> GetAllErrors()
        {
            return await _context.Errors
                .AsNoTracking()
                .ToListAsync();
        }
    }
}