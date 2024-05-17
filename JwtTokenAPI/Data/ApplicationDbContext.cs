using JwtTokenAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace JwtTokenAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        protected readonly IConfiguration configuration;
        public ApplicationDbContext(IConfiguration config, DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            configuration = config;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"));
        }

        public DbSet<User> Users => Set<User>();

    }
}
