﻿using Microsoft.EntityFrameworkCore;
using CoreLogic.Models;

namespace UsersService.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) 
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }

}
