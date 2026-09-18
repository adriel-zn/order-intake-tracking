namespace OrderIntake.Application.Orders;

public enum OrderCreationOutcome
{
    Created,
    DuplicateIdempotent,
    DuplicateConflict
}
