using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

}
