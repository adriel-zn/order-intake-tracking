using System.ComponentModel.DataAnnotations;
using OrderIntake.Api.Dtos.Requests;
using OrderIntake.Api.Tests.TestSupport;
using Xunit;

namespace OrderIntake.Api.Tests.Validation;

public class CreateOrderRequestValidationTests
{
    private static IList<ValidationResult> Validate(CreateOrderRequest request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, context, results, validateAllProperties: true);

        results.AddRange(ValidateNested(request.Customer));
        foreach (var lineItem in request.LineItems)
        {
            results.AddRange(ValidateNested(lineItem));
        }

        return results;
    }

    private static IList<ValidationResult> ValidateNested(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ValidRequest_HasNoValidationErrors()
    {
        var errors = Validate(RequestFactory.ValidOrder());

        Assert.Empty(errors);
    }

    [Fact]
    public void MissingExternalReference_IsInvalid()
    {
        var errors = Validate(RequestFactory.ValidOrder(externalReference: ""));

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateOrderRequest.ExternalReference)));
    }

    [Fact]
    public void NoLineItems_IsInvalid()
    {
        var errors = Validate(RequestFactory.ValidOrder(lineItems: new()));

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateOrderRequest.LineItems)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonPositiveQuantity_IsInvalid(int quantity)
    {
        var lineItem = RequestFactory.LineItem("CODE-1", "Item", quantity, unitPrice: 10m);

        var errors = ValidateNested(lineItem);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LineItemRequest.Quantity)));
    }

    [Fact]
    public void FractionalQuantityCannotBeRepresented_QuantityIsAWholeNumberByType()
    {
        Assert.Equal(typeof(int), typeof(LineItemRequest).GetProperty(nameof(LineItemRequest.Quantity))!.PropertyType);
    }

    [Fact]
    public void NegativeUnitPrice_IsInvalid()
    {
        var lineItem = RequestFactory.LineItem("CODE-1", "Item", quantity: 1, unitPrice: -0.01m);

        var errors = ValidateNested(lineItem);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(LineItemRequest.UnitPrice)));
    }

    [Fact]
    public void ZeroUnitPrice_IsValid()
    {
        var lineItem = RequestFactory.LineItem("CODE-1", "Item", quantity: 1, unitPrice: 0m);

        var errors = ValidateNested(lineItem);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("eur")]
    [InlineData("ZAR")]
    public void ThreeLetterCurrencyCodes_AreValid(string currency)
    {
        var errors = Validate(RequestFactory.ValidOrder(currency: currency));

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("DOLLARS")]
    [InlineData("")]
    public void NonThreeLetterCurrencyCodes_AreInvalid(string currency)
    {
        var errors = Validate(RequestFactory.ValidOrder(currency: currency));

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(CreateOrderRequest.Currency)));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void InvalidCustomerEmail_IsInvalid(string email)
    {
        var errors = Validate(RequestFactory.ValidOrder(customerEmail: email));

        Assert.Contains(errors, e => e.MemberNames.Contains($"{nameof(CustomerRequest.Email)}"));
    }
}
