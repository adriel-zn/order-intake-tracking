using System.ComponentModel.DataAnnotations;

namespace OrderIntake.Domain.Orders;

public sealed class Customer
{
    private Customer() { }
    public Customer(string email, string name)
    {
        Email = Guard.Text(email, nameof(Email), 320);
        Name = Guard.Text(name, nameof(Name), 200);
        if (!new EmailAddressAttribute().IsValid(Email))
            throw new DomainValidationException("Customer email is invalid.");
        Id = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
}
