using OrderIntake.Domain.Orders;
using OrderIntake.Application.Orders;
using Xunit;

namespace OrderIntake.Api.Tests.Services;

public class OrderStatusTransitionsTests
{
    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Fulfilled, true)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled, true)]
    public void CanTransition_AllowsSensibleSalesFlowMoves(OrderStatus from, OrderStatus to, bool expected)
    {
        Assert.Equal(expected, OrderStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Fulfilled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Pending)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Pending)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Fulfilled, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Fulfilled)]
    public void CanTransition_RejectsMovesThatDontMakeSense(OrderStatus from, OrderStatus to)
    {
        Assert.False(OrderStatusTransitions.CanTransition(from, to));
    }

    [Theory]
    [InlineData(OrderStatus.Fulfilled)]
    [InlineData(OrderStatus.Cancelled)]
    public void IsTerminal_TrueForFulfilledAndCancelled(OrderStatus status)
    {
        Assert.True(OrderStatusTransitions.IsTerminal(status));
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    public void IsTerminal_FalseForPendingAndConfirmed(OrderStatus status)
    {
        Assert.False(OrderStatusTransitions.IsTerminal(status));
    }

    [Fact]
    public void DescribeRejection_ForTerminalStatus_ExplainsItIsFinal()
    {
        var message = OrderStatusTransitions.DescribeRejection(OrderStatus.Fulfilled, OrderStatus.Pending);

        Assert.Contains("final", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Fulfilled", message);
    }

    [Fact]
    public void DescribeRejection_ForNonTerminalStatus_ListsAllowedNextStatuses()
    {
        var message = OrderStatusTransitions.DescribeRejection(OrderStatus.Pending, OrderStatus.Fulfilled);

        Assert.Contains("Confirmed", message);
        Assert.Contains("Cancelled", message);
    }
}
