namespace OrderIntake.Domain.Orders;

public static class OrderStatusTransitions
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> AllowedNextStatuses =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Pending] = new[] { OrderStatus.Confirmed, OrderStatus.Cancelled },
            [OrderStatus.Confirmed] = new[] { OrderStatus.Fulfilled, OrderStatus.Cancelled },
            [OrderStatus.Fulfilled] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
        };

    public static IReadOnlyList<OrderStatus> GetAllowedNextStatuses(OrderStatus current) =>
        Array.AsReadOnly(AllowedNextStatuses[current]);

    public static bool IsTerminal(OrderStatus status) => AllowedNextStatuses[status].Length == 0;

    public static bool CanTransition(OrderStatus from, OrderStatus to) =>
        AllowedNextStatuses[from].Contains(to);

    public static string DescribeRejection(OrderStatus from, OrderStatus to)
    {
        if (IsTerminal(from))
        {
            return $"Order is already '{from}', which is a final status and cannot be changed.";
        }

        var allowed = GetAllowedNextStatuses(from);
        return $"Cannot change order status from '{from}' to '{to}'. " +
               $"Allowed next status(es) from '{from}': {string.Join(", ", allowed)}.";
    }
}
