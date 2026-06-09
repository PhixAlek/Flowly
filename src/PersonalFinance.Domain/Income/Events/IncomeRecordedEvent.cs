using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Income.Events;

public sealed record IncomeRecordedEvent(
    Guid IncomeId,
    Guid MonthlyIncomeId,
    decimal GrossAmount,
    string Currency,
    DateTime OccurredAtUtc) : IDomainEvent;
