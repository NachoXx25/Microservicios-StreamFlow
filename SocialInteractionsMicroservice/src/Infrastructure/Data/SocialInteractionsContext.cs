using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SocialInteractionsMicroservice.src.Domain.Models;

namespace SocialInteractionsMicroservice.src.Infrastructure.Data
{
    public class SocialInteractionsContext : DbContext
    {

        public SocialInteractionsContext(DbContextOptions<SocialInteractionsContext> options)
            : base(options)
        {
            Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        }

        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoInteractions> VideoInteractions { get; set; }
    }
}