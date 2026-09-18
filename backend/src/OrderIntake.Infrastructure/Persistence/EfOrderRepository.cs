using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Infrastructure.Persistence;

public sealed class EfOrderRepository(OrderIntakeDbContext context) : IOrderRepository
{
    public (Order Order, bool WasCreated) GetOrCreateByReference(string normalizedExternalReference, Func<Order> factory)
    {
        var existing = GetByReference(normalizedExternalReference);
        if (existing is not null) return (existing, false);
        var candidate = factory();
        context.Orders.Add(candidate);
        try
        {
            context.SaveChanges();
            return (candidate, true);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            context.ChangeTracker.Clear();
            var winner = GetByReference(normalizedExternalReference);
            if (winner is null) throw;
            return (winner, false);
        }
    }

    public Order? GetById(Guid id) => context.Orders.SingleOrDefault(order => order.Id == id);
    public Order? GetByReference(string normalizedExternalReference) =>
        context.Orders.SingleOrDefault(order => order.ExternalReference == normalizedExternalReference);
    public IReadOnlyList<Order> GetAll() => context.Orders.AsNoTracking().ToList();

    public bool TrySaveChanges()
    {
        try
        {
            context.SaveChanges();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            context.ChangeTracker.Clear();
            return false;
        }
    }
}
