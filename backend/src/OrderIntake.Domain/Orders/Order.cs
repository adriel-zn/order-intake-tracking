using System.Text.RegularExpressions;

namespace OrderIntake.Domain.Orders;

public sealed class Order
{
    private readonly List<LineItem> _lineItems = new();
    private Order() { }

    public static Order Create(string externalReference, Customer customer,
        IEnumerable<LineItem> lineItems, string currency, string? notes, DateTimeOffset now)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            ExternalReference = Guard.Text(externalReference, nameof(ExternalReference), 200),
            Customer = customer ?? throw new DomainValidationException("Customer is required."),
            Currency = Guard.Text(currency, nameof(Currency), 3).ToUpperInvariant(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            Status = OrderStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
        if (!Regex.IsMatch(order.Currency, "^[A-Z]{3}$"))
            throw new DomainValidationException("Currency must contain three letters.");
        if (notes?.Length > 2000) throw new DomainValidationException("Notes must not exceed 2000 characters.");
        if (lineItems is null) throw new DomainValidationException("Line items are required.");
        order._lineItems.AddRange(lineItems);
        if (order._lineItems.Count == 0 || order._lineItems.Any(item => item is null))
            throw new DomainValidationException("At least one valid line item is required.");
        try { _ = order.Total; }
        catch (OverflowException) { throw new DomainValidationException("Order total exceeds the supported decimal range."); }
        return order;
    }

    public Guid Id { get; private set; }
    public string ExternalReference { get; private set; } = string.Empty;
    public Customer Customer { get; private set; } = null!;
    public IReadOnlyList<LineItem> LineItems => _lineItems.AsReadOnly();
    public string Currency { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public decimal Subtotal => _lineItems.Sum(item => item.LineTotal);
    public decimal Total => Subtotal;

    public string? ChangeStatus(OrderStatus next, DateTimeOffset now)
    {
        if (!Enum.IsDefined(next)) return "Unknown order status.";
        if (next == Status) return null;
        if (!OrderStatusTransitions.CanTransition(Status, next))
            return OrderStatusTransitions.DescribeRejection(Status, next);
        Status = next;
        UpdatedAt = now;
        return null;
    }

    public bool IsEquivalentSubmission(Order other)
    {
        static IEnumerable<(string Code, int Quantity, decimal Price)> Lines(Order order) =>
            order.LineItems.Select(item => (item.Code.ToUpperInvariant(), item.Quantity, item.UnitPrice))
                .OrderBy(item => item.Item1, StringComparer.Ordinal).ThenBy(item => item.Quantity).ThenBy(item => item.UnitPrice);
        return string.Equals(Customer.Email, other.Customer.Email, StringComparison.OrdinalIgnoreCase)
            && Currency == other.Currency && Lines(this).SequenceEqual(Lines(other));
    }
}
