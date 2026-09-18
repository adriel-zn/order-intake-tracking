using OrderIntake.Domain.Orders;

namespace OrderIntake.Application.Orders;

public record OrderCreationResult(OrderCreationOutcome Outcome, Order Order, string? Message = null);

public record OrderStatusChangeResult(StatusChangeOutcome Outcome, Order? Order, string? Message = null);
