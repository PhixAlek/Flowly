using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Common.ValueObjects;

/// <summary>
/// Represents an amount of money with currency.
/// Negative amounts are disallowed at creation — use domain methods to represent
/// reductions (e.g. deductions, taxes) explicitly on the owning entity.
/// Rule I3: "What if value is negative?" — Money enforces non-negative; callers must
/// model deductions through dedicated domain operations.
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }

    private Money(decimal amount, CurrencyCode currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, CurrencyCode currency)
    {
        if (amount < 0)
            return Result.Failure<Money>(Error.BusinessRule("NegativeAmount",
                "Money amount cannot be negative. Model deductions through domain operations."));

        return Result.Success(new Money(amount, currency));
    }

    public static Money Zero(CurrencyCode currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidOperationException("Resulting money amount would be negative.");
        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor) => new(Math.Round(Amount * factor, 2), Currency);

    public Money WithAmount(decimal amount) => new(amount, Currency);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot operate on different currencies: {Currency} vs {other.Currency}.");
    }

    protected override IEnumerable<object?> GetComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
