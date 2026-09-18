using OrderIntake.Api.Dtos.Requests;

namespace OrderIntake.Api.Tests.TestSupport;

public static class RequestFactory
{
    public static CreateOrderRequest ValidOrder(
        string externalReference = "PO-1001",
        string customerEmail = "buyer@acme.com",
        string customerName = "Acme Corp",
        string currency = "USD",
        string? notes = null,
        List<LineItemRequest>? lineItems = null)
    {
        return new CreateOrderRequest
        {
            ExternalReference = externalReference,
            Customer = new CustomerRequest { Email = customerEmail, Name = customerName },
            Currency = currency,
            Notes = notes,
            LineItems = lineItems ?? new List<LineItemRequest>
            {
                new() { Code = "WIDGET-1", Name = "Widget", Quantity = 3, UnitPrice = 19.99m },
                new() { Code = "GADGET-2", Name = "Gadget", Quantity = 1, UnitPrice = 49.50m }
            }
        };
    }

    public static LineItemRequest LineItem(string code, string name, int quantity, decimal unitPrice) =>
        new() { Code = code, Name = name, Quantity = quantity, UnitPrice = unitPrice };
}
