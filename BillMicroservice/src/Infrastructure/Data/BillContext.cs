using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BillMicroservice.src.Domain.Models;
using BillMicroservice.src.Domain.Models.Bill;
using BillMicroservice.src.Domain.Models.User;
using Microsoft.EntityFrameworkCore;

namespace BillMicroservice.src.Infrastructure.Data
{
    public class BillContext : DbContext
    {
        public BillContext(DbContextOptions<BillContext> options) : base(options)
        {
        }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<Status> Statuses { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }
    }
}