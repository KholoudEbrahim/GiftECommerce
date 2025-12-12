using System;
using System.Reflection.Metadata;
using CategoryService.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace CategoryService.DataBase;


public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    { }



    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Occasion> Occasions { get; set; }

    // Joins Table
    public DbSet<ProductOccasion> ProductOccasions { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogAssemblyRefrence).Assembly);
    }
}
