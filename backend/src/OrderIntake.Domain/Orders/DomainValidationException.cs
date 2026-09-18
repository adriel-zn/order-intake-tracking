namespace OrderIntake.Domain.Orders;

public sealed class DomainValidationException(string message) : Exception(message);

internal static class Guard
{
    public static string Text(string? value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
            throw new DomainValidationException($"{name} is required and must not exceed {maxLength} characters.");
        return value.Trim();
    }
}
