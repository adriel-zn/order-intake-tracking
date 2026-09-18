using OrderIntake.Domain.Orders;

namespace OrderIntake.Application.Orders;

public sealed class OrderService(IOrderRepository repository) : IOrderService
{
    public OrderCreationResult CreateOrder(CreateOrderCommand request)
    {
        var candidate = Order.Create(request.ExternalReference,
            new Customer(request.CustomerEmail, request.CustomerName),
            request.LineItems.Select(item => new LineItem(item.Code, item.Name, item.Quantity, item.UnitPrice)),
            request.Currency, request.Notes, DateTimeOffset.UtcNow);
        var (order, created) = repository.GetOrCreateByReference(candidate.ExternalReference, () => candidate);
        if (created) return new(OrderCreationOutcome.Created, order);
        if (order.IsEquivalentSubmission(candidate)) return new(OrderCreationOutcome.DuplicateIdempotent, order);
        return new(OrderCreationOutcome.DuplicateConflict, order,
            $"External reference '{candidate.ExternalReference}' was already used for a different order. Submit a new, unique external reference.");
    }

    public Order? GetById(Guid id) => repository.GetById(id);

    public IReadOnlyList<Order> ListOrders(OrderStatus? statusFilter = null) =>
        repository.GetAll().Where(order => !statusFilter.HasValue || order.Status == statusFilter)
            .OrderByDescending(order => order.CreatedAt).ToList();

    public OrderStatusChangeResult ChangeStatus(Guid id, OrderStatus newStatus)
    {
        var order = repository.GetById(id);
        if (order is null) return new(StatusChangeOutcome.NotFound, null, $"No order exists with id '{id}'.");
        var error = order.ChangeStatus(newStatus, DateTimeOffset.UtcNow);
        if (error is not null) return new(StatusChangeOutcome.InvalidTransition, order, error);
        if (!repository.TrySaveChanges())
            return new(StatusChangeOutcome.ConcurrencyConflict, null, "The order changed during this request. Reload it and retry.");
        return new(StatusChangeOutcome.Success, order);
    }
}
