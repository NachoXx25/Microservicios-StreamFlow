using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitoringMicroservice.src.Infrastructure.Data;
using MonitoringMicroservice.src.Infrastructure.Repositories.Interfaces;

namespace MonitoringMicroservice.src.Infrastructure.Repositories.Implements
{
    public class ActionRepository : IActionRepository
    {
        private readonly MonitoringContext _context;
        public ActionRepository(MonitoringContext context)
        {
            _context = context;
        }

        public async Task<List<Domain.Models.Action>> GetAllActions()
        {
            return await _context.Actions
                .AsNoTracking()
                .ToListAsync();
        }

    }
}