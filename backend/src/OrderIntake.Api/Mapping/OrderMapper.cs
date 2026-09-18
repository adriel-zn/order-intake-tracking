using OrderIntake.Api.Dtos.Requests;
using OrderIntake.Api.Dtos.Responses;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Mapping;

public static class OrderMapper
{
    public static OrderIntake.Application.Orders.CreateOrderCommand ToCommand(CreateOrderRequest request)
    {
        return new(request.ExternalReference, request.Customer.Email, request.Customer.Name,
            request.Currency, request.Notes,
            request.LineItems.Select(item => new OrderIntake.Application.Orders.CreateLineItem(
                item.Code, item.Name, item.Quantity, item.UnitPrice)).ToList());
    }

    public static OrderResponse ToResponse(Order order) => new()
    {
        Id = order.Id,
        ExternalReference = order.ExternalReference,
        Customer = new CustomerResponse
        {
            Id = order.Customer.Id,
            Email = order.Customer.Email,
            Name = order.Customer.Name
        },
        LineItems = order.LineItems.Select(li => new LineItemResponse
        {
            Code = li.Code,
            Name = li.Name,
            Quantity = li.Quantity,
            UnitPrice = li.UnitPrice,
            LineTotal = li.LineTotal
        }).ToList(),
        Currency = order.Currency,
        Notes = order.Notes,
        Status = order.Status,
        Subtotal = order.Subtotal,
        Total = order.Total,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt
    };

    public static OrderSummaryResponse ToSummary(Order order) => new()
    {
        Id = order.Id,
        ExternalReference = order.ExternalReference,
        CustomerName = order.Customer.Name,
        CustomerEmail = order.Customer.Email,
        Status = order.Status,
        Currency = order.Currency,
        Total = order.Total,
        LineItemCount = order.LineItems.Count,
        CreatedAt = order.CreatedAt,
        UpdatedAt = order.UpdatedAt
    };
}
