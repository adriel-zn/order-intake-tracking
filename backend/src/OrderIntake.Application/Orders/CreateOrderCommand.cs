namespace OrderIntake.Application.Orders;

public sealed record CreateOrderCommand(string ExternalReference, string CustomerEmail,
    string CustomerName, string Currency, string? Notes, IReadOnlyList<CreateLineItem> LineItems);

public sealed record CreateLineItem(string Code, string Name, int Quantity, decimal UnitPrice);
