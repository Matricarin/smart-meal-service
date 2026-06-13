using Microsoft.EntityFrameworkCore;

using SmartMealService.Domain.Models;

namespace SmartMealService.Infrastructure;

public sealed class MenuDbContext : DbContext
{
    public DbSet<SmsMenuItem> MenuItems { get; set; } = null!;

    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SmsMenuItem>(entity =>
        {
            entity.ToTable("menu_items");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.Article)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("article");
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.Price)
                .IsRequired()
                .HasColumnType("numeric(18, 2)")
                .HasColumnName("price");
        });
    }
}