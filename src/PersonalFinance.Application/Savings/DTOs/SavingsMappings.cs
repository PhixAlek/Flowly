using PersonalFinance.Domain.Savings.Entities;

namespace PersonalFinance.Application.Savings.DTOs;

internal static class SavingsMappings
{
    public static SavingsGoalDto ToDto(this SavingsGoal g) => new(
        g.Id, g.UserId, g.Name, g.GoalType,
        g.TargetAmount.Amount, g.CurrentAmount.Amount,
        g.TargetAmount.Currency.Value,
        g.ProgressPercent, g.IsAchieved,
        g.TargetDate, g.CreatedAtUtc);
}
