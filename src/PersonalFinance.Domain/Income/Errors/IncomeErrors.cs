using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Income.Errors;

public static class IncomeErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("Income", id);
    public static readonly Error MonthlyIncomeNotFound =
        new("Income.MonthlyNotFound", "Monthly income record not found.");
    public static readonly Error DuplicatePeriod =
        Error.Conflict("DuplicatePeriod", "A monthly income already exists for this period.");
    public static readonly Error NegativeAmount =
        Error.BusinessRule("NegativeAmount", "Income amount must be positive.");
}
