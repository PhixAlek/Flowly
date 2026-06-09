using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Enums;
using PersonalFinance.Domain.Income.Events;

namespace PersonalFinance.Domain.Income.Entities;

/// <summary>
/// A single income record within a MonthlyIncome.
/// Rule I1: multiple income entries per month are supported.
/// Rule I2: TaxInfo encapsulates gross/net.
/// Rule I6: OriginalCurrency tracks foreign currency; ConvertedAmount holds local equivalent.
/// </summary>
public sealed class IncomeEntry : Entity
{
    public string Source { get; private set; } = string.Empty;
    public IncomeType Type { get; private set; }
    public IncomeNature Nature { get; private set; }
    public ContractType ContractType { get; private set; }
    public TaxInfo TaxInfo { get; private set; } = null!;
    public CurrencyCode OriginalCurrency { get; private set; } = null!;
    public Money? ConvertedAmount { get; private set; }        // null until conversion applied
    public decimal? ExchangeRateUsed { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>Net amount in original currency.</summary>
    public decimal NetInOriginalCurrency => TaxInfo.NetAmount;

    private IncomeEntry() { }

    internal static Result<IncomeEntry> Create(
        string source,
        IncomeType type,
        ContractType contractType,
        decimal grossAmount,
        decimal deductionRatePercent,
        CurrencyCode originalCurrency,
        DateTime receivedDate,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(source))
            return Result.Failure<IncomeEntry>(Error.Validation("Source", "Income source is required."));

        var taxResult = TaxInfo.Create(grossAmount, deductionRatePercent);
        if (taxResult.IsFailure) return Result.Failure<IncomeEntry>(taxResult.Error);

        var entry = new IncomeEntry
        {
            Source = source.Trim(),
            Type = type,
            Nature = type.GetDefaultNature(),
            ContractType = contractType,
            TaxInfo = taxResult.Value,
            OriginalCurrency = originalCurrency,
            ReceivedDate = receivedDate,
            Notes = notes
        };

        return Result.Success(entry);
    }

    /// <summary>Apply a currency conversion. Rule I6.</summary>
    internal Result ApplyConversion(CurrencyCode targetCurrency, decimal exchangeRate)
    {
        if (exchangeRate <= 0)
            return Result.Failure(Error.Validation("ExchangeRate", "Exchange rate must be positive."));

        var converted = Math.Round(TaxInfo.NetAmount * exchangeRate, 2);
        var moneyResult = Money.Create(converted, targetCurrency);
        if (moneyResult.IsFailure) return Result.Failure(moneyResult.Error);

        ConvertedAmount = moneyResult.Value;
        ExchangeRateUsed = exchangeRate;
        Touch();

        Raise(new IncomeCurrencyConvertedEvent(
            Id, OriginalCurrency.Value, targetCurrency.Value,
            converted, exchangeRate, DateTime.UtcNow));

        return Result.Success();
    }
}
