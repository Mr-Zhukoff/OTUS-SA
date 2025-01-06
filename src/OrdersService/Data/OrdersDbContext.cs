using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Data;
public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }

}