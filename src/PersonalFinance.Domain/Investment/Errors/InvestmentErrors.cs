using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Investment.Errors;

public static class InvestmentErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("Investment", id);
    public static readonly Error AlreadyLiquidated =
        Error.BusinessRule("AlreadyLiquidated", "Investment has already been liquidated.");
    public static readonly Error NotMatured =
        Error.BusinessRule("NotMatured", "Investment has not yet reached its maturity date.");
}
