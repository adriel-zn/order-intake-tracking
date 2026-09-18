using Microsoft.AspNetCore.Mvc;
using OrderIntake.Api.Dtos.Requests;
using OrderIntake.Api.Dtos.Responses;
using OrderIntake.Api.Mapping;
using OrderIntake.Domain.Orders;
using OrderIntake.Application.Orders;

namespace OrderIntake.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<OrderResponse> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = _orderService.CreateOrder(OrderMapper.ToCommand(request));
        var response = OrderMapper.ToResponse(result.Order);

        return result.Outcome switch
        {
            OrderCreationOutcome.Created =>
                CreatedAtAction(nameof(GetOrder), new { id = result.Order.Id }, response),

            OrderCreationOutcome.DuplicateIdempotent =>
                Ok(response),

            OrderCreationOutcome.DuplicateConflict =>
                Conflict(new ProblemDetails
                {
                    Title = "Duplicate external reference",
                    Detail = result.Message,
                    Status = StatusCodes.Status409Conflict
                }),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<OrderResponse> GetOrder(Guid id)
    {
        var order = _orderService.GetById(id);
        if (order is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Order not found",
                Detail = $"No order exists with id '{id}'.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(OrderMapper.ToResponse(order));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<OrderSummaryResponse>> ListOrders([FromQuery] OrderStatus? status)
    {
        var orders = _orderService.ListOrders(status);
        return Ok(orders.Select(OrderMapper.ToSummary));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public ActionResult<OrderResponse> ChangeStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = _orderService.ChangeStatus(id, request.Status!.Value);

        return result.Outcome switch
        {
            StatusChangeOutcome.Success =>
                Ok(OrderMapper.ToResponse(result.Order!)),

            StatusChangeOutcome.NotFound =>
                NotFound(new ProblemDetails
                {
                    Title = "Order not found",
                    Detail = result.Message,
                    Status = StatusCodes.Status404NotFound
                }),

            StatusChangeOutcome.ConcurrencyConflict =>
                Conflict(new ProblemDetails { Title = "Concurrent order update", Detail = result.Message, Status = 409 }),

            StatusChangeOutcome.InvalidTransition =>
                Conflict(new ProblemDetails
                {
                    Title = "Invalid status transition",
                    Detail = result.Message,
                    Status = StatusCodes.Status409Conflict
                }),

            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
