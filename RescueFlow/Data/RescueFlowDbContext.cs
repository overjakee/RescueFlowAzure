using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Pipelines.Sockets.Unofficial.Arenas;
using RescueFlow.Models;

namespace RescueFlow.Data
{
    public class RescueFlowDbContext : DbContext
    {
        public RescueFlowDbContext(DbContextOptions<RescueFlowDbContext> options) : base(options)
        {
        }

        public DbSet<Area> Areas { get; set; }
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
    }
}
