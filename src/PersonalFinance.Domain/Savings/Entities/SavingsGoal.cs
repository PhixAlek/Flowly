using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Savings.Enums;
using PersonalFinance.Domain.Savings.Errors;
using PersonalFinance.Domain.Savings.Events;

namespace PersonalFinance.Domain.Savings.Entities;

/// <summary>Aggregate root: a savings goal (emergency fund, trip, etc.) with tracked deposits.</summary>
public sealed class SavingsGoal : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SavingsGoalType GoalType { get; private set; }
    public Money TargetAmount { get; private set; } = null!;
    public Money CurrentAmount { get; private set; } = null!;
    public DateTime? TargetDate { get; private set; }
    public bool IsAchieved => CurrentAmount.Amount >= TargetAmount.Amount;

    private SavingsGoal() { }

    public static Result<SavingsGoal> Create(
        Guid userId, string name, SavingsGoalType goalType,
        decimal targetAmount, CurrencyCode currency, DateTime? targetDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<SavingsGoal>(Error.Validation("Name", "Savings goal name is required."));

        var targetResult = Money.Create(targetAmount, currency);
        if (targetResult.IsFailure) return Result.Failure<SavingsGoal>(targetResult.Error);

        return Result.Success(new SavingsGoal
        {
            UserId = userId,
            Name = name.Trim(),
            GoalType = goalType,
            TargetAmount = targetResult.Value,
            CurrentAmount = Money.Zero(currency),
            TargetDate = targetDate
        });
    }

    public Result Deposit(decimal amount)
    {
        var moneyResult = Money.Create(amount, TargetAmount.Currency);
        if (moneyResult.IsFailure) return Result.Failure(moneyResult.Error);

        CurrentAmount = CurrentAmount.Add(moneyResult.Value);
        Raise(new SavingsDepositedEvent(Id, amount, TargetAmount.Currency.Value, DateTime.UtcNow));
        Touch();
        return Result.Success();
    }

    public Result Withdraw(decimal amount)
    {
        var moneyResult = Money.Create(amount, TargetAmount.Currency);
        if (moneyResult.IsFailure) return Result.Failure(moneyResult.Error);
        if (moneyResult.Value.Amount > CurrentAmount.Amount)
            return Result.Failure(SavingsErrors.InsufficientBalance);

        CurrentAmount = CurrentAmount.Subtract(moneyResult.Value);
        Touch();
        return Result.Success();
    }

    public decimal ProgressPercent =>
        TargetAmount.Amount > 0 ? Math.Round(CurrentAmount.Amount / TargetAmount.Amount * 100, 1) : 0;
}
