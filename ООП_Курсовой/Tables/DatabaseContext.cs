using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql;

namespace ООП_Курсовой.Tables;

public class DatabaseContext : DbContext
{
    private static NpgsqlDataSource? _dataSource;
    private static readonly object _lock = new object();

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<IngredientRoll> IngredientRolls { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<MenuSetComponent> MenuSetComponents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var host = ConfigurationManager.AppSettings["DbHost"] ?? "localhost";
            var port = ConfigurationManager.AppSettings["DbPort"] ?? "5432";
            var db = ConfigurationManager.AppSettings["DbName"] ?? "KaoriMau";
            var cs = $"Host={host};Port={port};Database={db};Username=postgres;Password=1111";

            if (_dataSource == null)
            {
                lock (_lock)
                {
                    if (_dataSource == null)
                    {
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(cs);
                        dataSourceBuilder.MapEnum<OrderStatus>("order_status");
                        dataSourceBuilder.MapEnum<DeliveryType>("delivery_type");
                        _dataSource = dataSourceBuilder.Build();
                    }
                }
            }

            optionsBuilder.UseNpgsql(_dataSource);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Phone).IsUnique();

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Client" }
        );

        modelBuilder.Entity<IngredientRoll>()
        .HasOne(ir => ir.Roll)
        .WithMany(r => r.IngredientRolls)
        .HasForeignKey(ir => ir.RollId);

        modelBuilder.Entity<IngredientRoll>()
            .HasOne(ir => ir.Ingredient)
            .WithMany(i => i.IngredientRolls)
            .HasForeignKey(ir => ir.IngredientId);

        modelBuilder.HasPostgresEnum<OrderStatus>("order_status");
        modelBuilder.HasPostgresEnum<DeliveryType>("delivery_type");

        var orderStatusConverter = new ValueConverter<OrderStatus, string>(
            v => v.ToString().ToLowerInvariant(),
            v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v, true));

        var deliveryTypeConverter = new ValueConverter<DeliveryType, string>(
            v => v.ToString().ToLowerInvariant(),
            v => (DeliveryType)Enum.Parse(typeof(DeliveryType), v, true));

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion(orderStatusConverter)
            .HasColumnType("order_status");

        modelBuilder.Entity<Order>()
            .Property(o => o.DeliveryType)
            .HasConversion(deliveryTypeConverter)
            .HasColumnType("delivery_type");

    }
}
