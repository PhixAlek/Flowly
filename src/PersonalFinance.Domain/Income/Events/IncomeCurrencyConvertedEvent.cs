using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Income.Events;

public sealed record IncomeCurrencyConvertedEvent(
    Guid IncomeId,
    string FromCurrency,
    string ToCurrency,
    decimal ConvertedAmount,
    decimal ExchangeRate,
    DateTime OccurredAtUtc) : IDomainEvent;
