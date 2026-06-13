using Microsoft.EntityFrameworkCore;

using SmartMealService.GuiClient.Models;

namespace SmartMealService.GuiClient.Infrastructure;

public sealed class VariablesDbContext : DbContext
{
    public DbSet<CustomEnvironmentVariable> EnvironmentVariables { get; set; } = null!;

    public VariablesDbContext(DbContextOptions<VariablesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomEnvironmentVariable>(entity =>
        {
            entity.ToTable("variables");

            entity.HasKey(e => e.Key);

            entity.Property(e => e.Key)
                .ValueGeneratedNever()
                .HasMaxLength(255)
                .HasColumnName("key");

            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("value");


            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
        });
    }
}