using OrderIntake.Domain.Orders;

namespace OrderIntake.Api.Tests.Domain;

public class OrderTests
{
    private static Order Create(params LineItem[] items) => Order.Create(" PO-1 ",
        new Customer("buyer@example.com", "Buyer"), items, "zar", null, DateTimeOffset.UtcNow);

    [Fact]
    public void EmptyOrderCannotBeCreatedOutsideTheApi() =>
        Assert.Throws<DomainValidationException>(() => Create());

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void InvalidLineCannotBeCreatedOutsideTheApi(int quantity, decimal price) =>
        Assert.Throws<DomainValidationException>(() => new LineItem("CODE", "Item", quantity, price));

    [Fact]
    public void UnrepresentableSqlPriceIsRejectedInsteadOfRounded() =>
        Assert.Throws<DomainValidationException>(() => new LineItem("CODE", "Item", 1, 0.000000001m));

    [Fact]
    public void FailedTransitionAndReplayLeaveTimestampUnchanged()
    {
        var order = Create(new LineItem("A", "Item", 1, 2));
        var original = order.UpdatedAt;
        Assert.NotNull(order.ChangeStatus(OrderStatus.Fulfilled, original.AddMinutes(1)));
        Assert.Null(order.ChangeStatus(OrderStatus.Pending, original.AddMinutes(2)));
        Assert.Equal(original, order.UpdatedAt);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void ReorderedRepeatedCodesAreEquivalent()
    {
        var first = Create(new LineItem("A", "First", 1, 2), new LineItem("A", "Second", 3, 4));
        var replay = Create(new LineItem("a", "Changed name", 3, 4), new LineItem("A", "First", 1, 2));
        Assert.True(first.IsEquivalentSubmission(replay));
    }

    [Fact]
    public void CallerCannotModifyOwnedLineCollection()
    {
        var lines = new List<LineItem> { new("A", "Item", 1, 2) };
        var order = Order.Create("PO-1", new Customer("buyer@example.com", "Buyer"), lines, "USD", null, DateTimeOffset.UtcNow);
        lines.Clear();
        Assert.Single(order.LineItems);
        Assert.Throws<NotSupportedException>(() => ((IList<LineItem>)order.LineItems).Clear());
    }
}
