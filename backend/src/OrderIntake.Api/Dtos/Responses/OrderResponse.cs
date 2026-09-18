using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Dtos.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }
    public string ExternalReference { get; set; } = string.Empty;
    public CustomerResponse Customer { get; set; } = null!;
    public List<LineItemResponse> LineItems { get; set; } = new();
    public string Currency { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
