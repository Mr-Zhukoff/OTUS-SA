using Microsoft.EntityFrameworkCore;
using System;
using UserServiceAPI.Models;

namespace UserServiceAPI.Data
{
    public class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }

}
