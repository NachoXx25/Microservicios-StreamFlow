using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MonitoringMicroservice.src.Infrastructure.Data
{
    public class MonitoringContext : DbContext
    {
        public MonitoringContext(DbContextOptions<MonitoringContext> options) : base(options)
        {
            Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        }

        public DbSet<Domain.Models.Action> Actions { get; set; } = null!;
        public DbSet<Domain.Models.Error> Errors { get; set; } = null!;
    }
}