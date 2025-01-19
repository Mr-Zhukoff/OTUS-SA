using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace BackgroundWorkerService.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Order> Orders { get; set; }
}
