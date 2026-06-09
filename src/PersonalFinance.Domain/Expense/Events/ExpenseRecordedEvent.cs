using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Expense.Events;

public sealed record ExpenseRecordedEvent(
    Guid ExpenseId,
    Guid MonthlyBudgetId,
    decimal Amount,
    string Currency,
    string Category,
    DateTime OccurredAtUtc) : IDomainEvent;
