using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Common.ValueObjects;

/// <summary>
/// ISO 4217 currency code — strongly typed to prevent raw string bugs.
/// Rule I6: supports freelancers paid in foreign currencies.
/// </summary>
public sealed class CurrencyCode : ValueObject
{
    public string Value { get; }

    // Common codes as static factories
    public static readonly CurrencyCode COP = new("COP");
    public static readonly CurrencyCode USD = new("USD");
    public static readonly CurrencyCode EUR = new("EUR");

    private CurrencyCode(string value) => Value = value;

    public static Result<CurrencyCode> Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            return Result.Failure<CurrencyCode>(Error.Validation("CurrencyCode",
                "Currency code must be a 3-letter ISO 4217 code (e.g. USD, COP, EUR)."));

        return Result.Success(new CurrencyCode(code.ToUpperInvariant()));
    }

    protected override IEnumerable<object?> GetComponents() { yield return Value; }
    public override string ToString() => Value;
}
