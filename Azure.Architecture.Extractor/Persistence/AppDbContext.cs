using Microsoft.EntityFrameworkCore;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Persistence;

internal class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options)
        : base(options)
    { }

    public DbSet<Service> Services { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>()
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        modelBuilder.Entity<Service>().HasKey(s => s.Name);
        modelBuilder.Entity<Service>().HasMany(x => x.Dependencies)
            .WithOne()
            .HasForeignKey("Origin");

        modelBuilder.Entity<ServiceDependency>()
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        modelBuilder.Entity<ServiceDependency>().Property<int>("Id");
        modelBuilder.Entity<ServiceDependency>().HasKey("Id");
    }
}