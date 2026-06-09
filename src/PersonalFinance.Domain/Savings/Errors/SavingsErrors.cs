using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Savings.Errors;

public static class SavingsErrors
{
    public static Error NotFound(Guid id) => Error.NotFound("SavingsGoal", id);
    public static readonly Error GoalAlreadyAchieved =
        Error.BusinessRule("GoalAchieved", "Savings goal has already been achieved.");
    public static readonly Error InsufficientBalance =
        Error.BusinessRule("InsufficientBalance", "Withdrawal exceeds current saved amount.");
}
