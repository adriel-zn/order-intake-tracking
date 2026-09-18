using System.ComponentModel.DataAnnotations;

namespace OrderIntake.Api.Dtos.Requests;

public class LineItemRequest
{
    [Required(ErrorMessage = "lineItems[].code is required.")]
    [MinLength(1, ErrorMessage = "lineItems[].code cannot be blank.")]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "lineItems[].name is required.")]
    [MinLength(1, ErrorMessage = "lineItems[].name cannot be blank.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "lineItems[].quantity must be a positive whole number.")]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335",
        ErrorMessage = "lineItems[].unitPrice must be zero or greater.")]
    public decimal UnitPrice { get; set; }
}
