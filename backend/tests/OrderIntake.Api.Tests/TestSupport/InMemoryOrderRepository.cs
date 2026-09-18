using System.Collections.Concurrent;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Tests.TestSupport;

public class InMemoryOrderRepository : IOrderRepository
{
    public bool TrySaveChanges() => true;
    private readonly ConcurrentDictionary<Guid, Order> _ordersById = new();
    private readonly ConcurrentDictionary<string, Guid> _idByReference = new(StringComparer.OrdinalIgnoreCase);

    private readonly object _createLock = new();

    public (Order Order, bool WasCreated) GetOrCreateByReference(string normalizedExternalReference, Func<Order> factory)
    {
        lock (_createLock)
        {
            if (_idByReference.TryGetValue(normalizedExternalReference, out var existingId)
                && _ordersById.TryGetValue(existingId, out var existingOrder))
            {
                return (existingOrder, false);
            }

            var newOrder = factory();
            _ordersById[newOrder.Id] = newOrder;
            _idByReference[normalizedExternalReference] = newOrder.Id;
            return (newOrder, true);
        }
    }

    public Order? GetById(Guid id) =>
        _ordersById.TryGetValue(id, out var order) ? order : null;

    public Order? GetByReference(string normalizedExternalReference) =>
        _idByReference.TryGetValue(normalizedExternalReference, out var id) ? GetById(id) : null;

    public IReadOnlyList<Order> GetAll() => _ordersById.Values.ToList();
}
