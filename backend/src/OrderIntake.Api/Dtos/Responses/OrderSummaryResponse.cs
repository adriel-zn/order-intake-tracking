using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Dtos.Responses;

public class OrderSummaryResponse
{
    public Guid Id { get; set; }
    public string ExternalReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int LineItemCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
