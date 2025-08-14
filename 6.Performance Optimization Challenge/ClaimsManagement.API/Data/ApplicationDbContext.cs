using ClaimsManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ClaimsManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Claim> Claims { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Policy> Policies { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Claim>()
            .HasOne(c => c.Policy)
            .WithMany(p => p.Claims)
            .HasForeignKey(c => c.PolicyId);

        modelBuilder.Entity<Claim>()
            .HasOne(c => c.User)
            .WithMany(u => u.Claims)
            .HasForeignKey(c => c.UserId);
    }
}
