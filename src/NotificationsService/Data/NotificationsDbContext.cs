using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace NotificationsService.Data;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Notification> Notifications { get; set; }

}
