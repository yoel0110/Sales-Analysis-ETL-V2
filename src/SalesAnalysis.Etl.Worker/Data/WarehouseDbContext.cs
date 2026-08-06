using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Etl.Worker.Data.Entities;

namespace SalesAnalysis.Etl.Worker.Data;

public sealed class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options)
    {
    }

    public DbSet<CustomerDim> CustomerDims => Set<CustomerDim>();
    public DbSet<ProductDim> ProductDims => Set<ProductDim>();
    public DbSet<DateDim> DateDims => Set<DateDim>();
    public DbSet<FactTable> Facts => Set<FactTable>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerDim>(entity =>
        {
            entity.ToTable("CustomerDim");
            entity.HasKey(e => e.CustomerDimId);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.CountryName).HasMaxLength(100);
            entity.Property(e => e.CityName).HasMaxLength(100);
        });

        modelBuilder.Entity<ProductDim>(entity =>
        {
            entity.ToTable("ProductDim");
            entity.HasKey(e => e.ProductDimId);
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<DateDim>(entity =>
        {
            entity.ToTable("DateDim");
            entity.HasKey(e => e.DateDimId);
            entity.Property(e => e.Fecha).HasColumnType("date");
            entity.Property(e => e.DayName).HasMaxLength(15);
            entity.Property(e => e.MonthName).HasMaxLength(15);
            entity.Property(e => e.Quarters).HasMaxLength(2);
        });

        modelBuilder.Entity<FactTable>(entity =>
        {
            entity.ToTable("FactTable");
            entity.HasKey(e => e.FactId);
            entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
        });
    }
}
