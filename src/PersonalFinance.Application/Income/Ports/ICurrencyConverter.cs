namespace PersonalFinance.Application.Income.Ports;

/// <summary>
/// Port for exchange rate lookup. Infrastructure provides the real adapter (ExchangeRate-API).
/// Rule I6: allows freelancers to convert USD/EUR to COP.
/// </summary>
public interface ICurrencyConverter
{
    /// <summary>Returns how many units of <paramref name="to"/> equal 1 unit of <paramref name="from"/>.</summary>
    Task<decimal> GetRateAsync(string from, string to, CancellationToken ct = default);

    /// <summary>Convert <paramref name="amount"/> from one currency to another.</summary>
    Task<decimal> ConvertAsync(decimal amount, string from, string to, CancellationToken ct = default);
}
