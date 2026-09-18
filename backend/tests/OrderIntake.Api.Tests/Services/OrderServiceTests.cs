using OrderIntake.Domain.Orders;
using OrderIntake.Application.Orders;
using OrderIntake.Api.Tests.TestSupport;
using Xunit;

namespace OrderIntake.Api.Tests.Services;

public class OrderServiceTests
{
    private static OrderCreationResult CreateOrder(OrderService service, OrderIntake.Api.Dtos.Requests.CreateOrderRequest request) => service.CreateOrder(OrderIntake.Api.Mapping.OrderMapper.ToCommand(request));

    private static OrderService NewService() => new(new InMemoryOrderRepository());

    [Fact]
    public void CreateOrder_NewReference_CreatesOrderInPendingStatus()
    {
        var service = NewService();

        var result = CreateOrder(service, RequestFactory.ValidOrder());

        Assert.Equal(OrderCreationOutcome.Created, result.Outcome);
        Assert.Equal(OrderStatus.Pending, result.Order.Status);
        Assert.NotEqual(Guid.Empty, result.Order.Id);
    }

    [Fact]
    public void CreateOrder_ComputesLineTotalsSubtotalAndTotalOnTheServer()
    {
        var service = NewService();
        var request = RequestFactory.ValidOrder(lineItems: new()
        {
            RequestFactory.LineItem("A", "Item A", quantity: 3, unitPrice: 10.00m),
            RequestFactory.LineItem("B", "Item B", quantity: 2, unitPrice: 4.25m)
        });

        var result = CreateOrder(service, request);

        Assert.Equal(30.00m, result.Order.LineItems[0].LineTotal);
        Assert.Equal(8.50m, result.Order.LineItems[1].LineTotal);
        Assert.Equal(38.50m, result.Order.Subtotal);
        Assert.Equal(38.50m, result.Order.Total);
    }

    [Fact]
    public void CreateOrder_TrimsExternalReferenceBeforeStoring()
    {
        var service = NewService();

        var result = CreateOrder(service, RequestFactory.ValidOrder(externalReference: "  PO-9  "));

        Assert.Equal("PO-9", result.Order.ExternalReference);
    }

    [Fact]
    public void CreateOrder_RepeatSubmissionOfSameReferenceWithSameDetails_DoesNotCreateDuplicate()
    {
        var service = NewService();
        var request = RequestFactory.ValidOrder(externalReference: "PO-1001");

        var first = CreateOrder(service, request);
        var second = CreateOrder(service, request);

        Assert.Equal(OrderCreationOutcome.Created, first.Outcome);
        Assert.Equal(OrderCreationOutcome.DuplicateIdempotent, second.Outcome);
        Assert.Equal(first.Order.Id, second.Order.Id);
        Assert.Single(service.ListOrders());
    }

    [Fact]
    public void CreateOrder_SameReferenceDifferentLineItems_IsReportedAsConflictNotSilentlyMerged()
    {
        var service = NewService();
        CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-1001"));

        var conflicting = CreateOrder(service, RequestFactory.ValidOrder(
            externalReference: "PO-1001",
            lineItems: new() { RequestFactory.LineItem("WIDGET-1", "Widget", quantity: 999, unitPrice: 19.99m) }));

        Assert.Equal(OrderCreationOutcome.DuplicateConflict, conflicting.Outcome);
        Assert.NotNull(conflicting.Message);
        Assert.Single(service.ListOrders());
    }

    [Fact]
    public void CreateOrder_SameReferenceDifferentCustomerEmail_IsReportedAsConflict()
    {
        var service = NewService();
        CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-1001", customerEmail: "a@acme.com"));

        var conflicting = CreateOrder(service,
            RequestFactory.ValidOrder(externalReference: "PO-1001", customerEmail: "b@acme.com"));

        Assert.Equal(OrderCreationOutcome.DuplicateConflict, conflicting.Outcome);
    }

    [Fact]
    public void CreateOrder_ExternalReferenceComparisonIsCaseInsensitive()
    {
        var service = NewService();
        CreateOrder(service, RequestFactory.ValidOrder(externalReference: "po-1001"));

        var result = CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-1001"));

        Assert.Equal(OrderCreationOutcome.DuplicateIdempotent, result.Outcome);
        Assert.Single(service.ListOrders());
    }

    [Fact]
    public void CreateOrder_ConcurrentSubmissionsOfSameReference_OnlyOneOrderIsCreated()
    {
        var service = NewService();
        const int attempts = 25;

        var results = new OrderCreationResult[attempts];
        Parallel.For(0, attempts, i =>
        {
            results[i] = CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-RACE"));
        });

        Assert.Single(service.ListOrders());
        Assert.Equal(1, results.Count(r => r.Outcome == OrderCreationOutcome.Created));
        Assert.Equal(attempts - 1, results.Count(r => r.Outcome == OrderCreationOutcome.DuplicateIdempotent));
    }

    [Fact]
    public void ListOrders_ReturnsNewestFirst()
    {
        var service = NewService();
        CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-1"));
        CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-2"));
        CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-3"));

        var orders = service.ListOrders();

        Assert.Equal(3, orders.Count);
        Assert.True(orders[0].CreatedAt >= orders[1].CreatedAt);
        Assert.True(orders[1].CreatedAt >= orders[2].CreatedAt);
        Assert.Equal("PO-3", orders[0].ExternalReference);
    }

    [Fact]
    public void ListOrders_CanFilterByStatus()
    {
        var service = NewService();
        var pending = CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-1")).Order;
        var toConfirm = CreateOrder(service, RequestFactory.ValidOrder(externalReference: "PO-2")).Order;
        service.ChangeStatus(toConfirm.Id, OrderStatus.Confirmed);

        var pendingOnly = service.ListOrders(OrderStatus.Pending);

        Assert.Single(pendingOnly);
        Assert.Equal(pending.Id, pendingOnly[0].Id);
    }

    [Fact]
    public void ChangeStatus_UnknownOrderId_ReturnsNotFound()
    {
        var service = NewService();

        var result = service.ChangeStatus(Guid.NewGuid(), OrderStatus.Confirmed);

        Assert.Equal(StatusChangeOutcome.NotFound, result.Outcome);
        Assert.Null(result.Order);
    }

    [Fact]
    public void ChangeStatus_ValidTransition_UpdatesStatusAndTimestamp()
    {
        var service = NewService();
        var order = CreateOrder(service, RequestFactory.ValidOrder()).Order;
        var originalUpdatedAt = order.UpdatedAt;

        var result = service.ChangeStatus(order.Id, OrderStatus.Confirmed);

        Assert.Equal(StatusChangeOutcome.Success, result.Outcome);
        Assert.Equal(OrderStatus.Confirmed, result.Order!.Status);
        Assert.True(result.Order.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ChangeStatus_InvalidTransition_ReturnsHelpfulMessageAndLeavesStatusUnchanged()
    {
        var service = NewService();
        var order = CreateOrder(service, RequestFactory.ValidOrder()).Order;

        var result = service.ChangeStatus(order.Id, OrderStatus.Fulfilled);

        Assert.Equal(StatusChangeOutcome.InvalidTransition, result.Outcome);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
        Assert.Equal(OrderStatus.Pending, service.GetById(order.Id)!.Status);
    }

    [Fact]
    public void ChangeStatus_TerminalStatus_CannotBeChangedAgain()
    {
        var service = NewService();
        var order = CreateOrder(service, RequestFactory.ValidOrder()).Order;
        service.ChangeStatus(order.Id, OrderStatus.Confirmed);
        service.ChangeStatus(order.Id, OrderStatus.Cancelled);

        var result = service.ChangeStatus(order.Id, OrderStatus.Confirmed);

        Assert.Equal(StatusChangeOutcome.InvalidTransition, result.Outcome);
        Assert.Contains("final", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ChangeStatus_RequestingTheCurrentStatusAgain_IsATreatedAsHarmlessNoOp()
    {
        var service = NewService();
        var order = CreateOrder(service, RequestFactory.ValidOrder()).Order;

        var result = service.ChangeStatus(order.Id, OrderStatus.Pending);

        Assert.Equal(StatusChangeOutcome.Success, result.Outcome);
        Assert.Equal(OrderStatus.Pending, result.Order!.Status);
    }
}
