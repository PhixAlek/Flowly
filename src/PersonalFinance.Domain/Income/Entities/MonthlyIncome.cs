using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Income.Enums;
using PersonalFinance.Domain.Income.Events;

namespace PersonalFinance.Domain.Income.Entities;

/// <summary>
/// Aggregate root for income — groups all income entries for a specific month+year.
/// Answers: "How much did I earn this month, from where, and in what currency?"
/// </summary>
public sealed class MonthlyIncome : AggregateRoot
{
    public Guid UserId { get; private set; }
    public DatePeriod Period { get; private set; } = null!;
    public CurrencyCode HomeCurrency { get; private set; } = null!;

    private readonly List<IncomeEntry> _entries = [];
    public IReadOnlyCollection<IncomeEntry> Entries => _entries.AsReadOnly();

    public decimal TotalGross => _entries.Sum(e => e.TaxInfo.GrossAmount);
    public decimal TotalNet => _entries.Sum(e => e.TaxInfo.NetAmount);
    public decimal TotalDeductions => _entries.Sum(e => e.TaxInfo.DeductionAmount);

    /// <summary>Total net in home currency, including converted entries.</summary>
    public decimal TotalNetInHomeCurrency =>
        _entries.Sum(e => e.ConvertedAmount?.Amount ?? e.NetInOriginalCurrency);

    private MonthlyIncome() { }

    public static Result<MonthlyIncome> Create(Guid userId, DatePeriod period, CurrencyCode homeCurrency)
    {
        if (userId == Guid.Empty)
            return Result.Failure<MonthlyIncome>(Error.Validation("UserId", "UserId is required."));

        return Result.Success(new MonthlyIncome
        {
            UserId = userId,
            Period = period,
            HomeCurrency = homeCurrency
        });
    }

    /// <summary>
    /// Adds a new income entry. Rule I1: multiple sources supported.
    /// </summary>
    public Result<IncomeEntry> AddEntry(
        string source,
        IncomeType type,
        ContractType contractType,
        decimal grossAmount,
        decimal deductionRatePercent,
        CurrencyCode currency,
        DateTime receivedDate,
        string? notes = null)
    {
        if (!Period.Contains(receivedDate))
            return Result.Failure<IncomeEntry>(Error.BusinessRule("WrongPeriod",
                $"Received date {receivedDate:yyyy-MM-dd} does not fall within period {Period}."));

        var entryResult = IncomeEntry.Create(
            source, type, contractType,
            grossAmount, deductionRatePercent,
            currency, receivedDate, notes);

        if (entryResult.IsFailure) return Result.Failure<IncomeEntry>(entryResult.Error);

        _entries.Add(entryResult.Value);
        Raise(new IncomeRecordedEvent(entryResult.Value.Id, Id, grossAmount, currency.Value, DateTime.UtcNow));
        Touch();

        return Result.Success(entryResult.Value);
    }

    /// <summary>Apply exchange rate to a specific entry. Rule I6.</summary>
    public Result ConvertEntry(Guid entryId, CurrencyCode targetCurrency, decimal exchangeRate)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is null)
            return Result.Failure(Error.NotFound("IncomeEntry", entryId));

        return entry.ApplyConversion(targetCurrency, exchangeRate);
    }
}
