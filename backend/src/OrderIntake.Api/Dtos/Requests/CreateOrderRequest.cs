using System.ComponentModel.DataAnnotations;

namespace OrderIntake.Api.Dtos.Requests;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "externalReference is required.")]
    [MinLength(1, ErrorMessage = "externalReference cannot be blank.")]
    [MaxLength(200)]
    public string ExternalReference { get; set; } = string.Empty;

    [Required(ErrorMessage = "customer is required.")]
    public CustomerRequest Customer { get; set; } = null!;

    [Required(ErrorMessage = "lineItems is required.")]
    [MinLength(1, ErrorMessage = "At least one line item is required.")]
    public List<LineItemRequest> LineItems { get; set; } = new();

    [Required(ErrorMessage = "currency is required.")]
    [RegularExpression("^[A-Za-z]{3}$", ErrorMessage = "currency must be a 3-letter ISO 4217 code, e.g. USD.")]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "notes cannot exceed 2000 characters.")]
    public string? Notes { get; set; }
}
