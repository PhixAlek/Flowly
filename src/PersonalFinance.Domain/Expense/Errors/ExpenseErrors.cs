using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Expense.Errors;

public static class ExpenseErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("Expense", id);
    public static readonly Error NegativeAmount =
        Error.BusinessRule("NegativeAmount", "Expense amount must be positive.");
    public static readonly Error FutureDateNotAllowed =
        Error.BusinessRule("FutureDate", "Expense date cannot be in the future.");
}
