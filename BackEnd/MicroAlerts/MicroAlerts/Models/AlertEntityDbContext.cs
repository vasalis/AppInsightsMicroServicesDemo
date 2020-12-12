using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroAlerts
{
    public class AlertEntityDbContext : DbContext
    {
        public DbSet<AlertEntity> Alerts { get; set; }

        public AlertEntityDbContext(DbContextOptions<AlertEntityDbContext> options) : base(options)
        {
            // Init Db if neeeded.
            Database.EnsureCreated();
        }

        public List<AlertEntity> getAlerts() => Alerts.ToList<AlertEntity>();        
    }
}
