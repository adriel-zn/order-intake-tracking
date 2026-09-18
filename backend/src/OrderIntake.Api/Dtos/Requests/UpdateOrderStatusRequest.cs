using System.ComponentModel.DataAnnotations;
using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Dtos.Requests;

public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "status is required.")]
    [EnumDataType(typeof(OrderStatus), ErrorMessage = "status must be a defined order status.")]
    public OrderStatus? Status { get; set; }
}
