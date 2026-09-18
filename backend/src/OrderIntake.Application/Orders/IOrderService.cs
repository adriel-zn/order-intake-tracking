
using OrderIntake.Domain.Orders;

namespace OrderIntake.Application.Orders;

public interface IOrderService
{
    OrderCreationResult CreateOrder(CreateOrderCommand request);

    Order? GetById(Guid id);

    IReadOnlyList<Order> ListOrders(OrderStatus? statusFilter = null);

    OrderStatusChangeResult ChangeStatus(Guid id, OrderStatus newStatus);
}
