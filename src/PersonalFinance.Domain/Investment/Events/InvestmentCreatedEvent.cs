using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Investment.Events;

public sealed record InvestmentCreatedEvent(
    Guid InvestmentId,
    string InvestmentType,
    decimal Principal,
    string Currency,
    DateTime OccurredAtUtc) : IDomainEvent;
