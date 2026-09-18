using Microsoft.EntityFrameworkCore;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Infrastructure.Persistence;

public sealed class OrderIntakeDbContext(DbContextOptions<OrderIntakeDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderIntakeDbContext).Assembly);
}
