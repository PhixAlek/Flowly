using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Savings.Events;

public sealed record SavingsDepositedEvent(
    Guid SavingsGoalId,
    decimal Amount,
    string Currency,
    DateTime OccurredAtUtc) : IDomainEvent;
