using OrderIntake.Domain.Orders;

namespace OrderIntake.Domain.Orders;

public interface IOrderRepository
{
    bool TrySaveChanges();
    (Order Order, bool WasCreated) GetOrCreateByReference(string normalizedExternalReference, Func<Order> factory);

    Order? GetById(Guid id);

    Order? GetByReference(string normalizedExternalReference);

    IReadOnlyList<Order> GetAll();
}
