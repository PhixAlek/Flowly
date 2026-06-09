using PersonalFinance.Domain.Savings.Enums;

namespace PersonalFinance.Application.Savings.DTOs;

public sealed record SavingsGoalDto(
    Guid Id,
    Guid UserId,
    string Name,
    SavingsGoalType GoalType,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Currency,
    decimal ProgressPercent,
    bool IsAchieved,
    DateTime? TargetDate,
    DateTime CreatedAtUtc);
