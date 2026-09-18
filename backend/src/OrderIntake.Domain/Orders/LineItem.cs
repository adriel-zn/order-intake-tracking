namespace OrderIntake.Domain.Orders;

public sealed class LineItem
{
    private LineItem() { }
    public LineItem(string code, string name, int quantity, decimal unitPrice)
    {
        Code = Guard.Text(code, nameof(Code), 100);
        Name = Guard.Text(name, nameof(Name), 200);
        if (quantity <= 0) throw new DomainValidationException("Quantity must be positive.");
        if (unitPrice < 0 || unitPrice >= 100000000000000000000m || decimal.Round(unitPrice, 8) != unitPrice)
            throw new DomainValidationException("Unit price must be nonnegative, below 10^20, and have at most 8 decimal places.");
        Id = Guid.NewGuid();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
