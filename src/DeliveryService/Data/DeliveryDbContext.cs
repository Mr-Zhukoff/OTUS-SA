using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Data;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Delivery> Deliveries { get; set; }

}
