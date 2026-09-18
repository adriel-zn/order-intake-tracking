namespace OrderIntake.Application.Orders;

public enum StatusChangeOutcome
{
    ConcurrencyConflict,
    Success,
    NotFound,
    InvalidTransition
}
