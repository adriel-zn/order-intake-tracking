using System.ComponentModel.DataAnnotations;

namespace OrderIntake.Api.Dtos.Requests;

public class CustomerRequest
{
    [Required(ErrorMessage = "customer.email is required.")]
    [EmailAddress(ErrorMessage = "customer.email must be a valid email address.")]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "customer.name is required.")]
    [MinLength(1, ErrorMessage = "customer.name cannot be blank.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
