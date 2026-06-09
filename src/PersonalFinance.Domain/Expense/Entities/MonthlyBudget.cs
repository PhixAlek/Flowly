using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Expense.Enums;
using PersonalFinance.Domain.Expense.Events;

namespace PersonalFinance.Domain.Expense.Entities;

/// <summary>
/// Aggregate root grouping all expenses for one month.
/// Links back to MonthlyIncome so the app knows the budget baseline.
/// </summary>
public sealed class MonthlyBudget : AggregateRoot
{
    public Guid UserId { get; private set; }
    public DatePeriod Period { get; private set; } = null!;
    public Guid? MonthlyIncomeId { get; private set; }
    public CurrencyCode HomeCurrency { get; private set; } = null!;

    private readonly List<ExpenseEntry> _entries = [];
    public IReadOnlyCollection<ExpenseEntry> Entries => _entries.AsReadOnly();

    public decimal TotalSpent => _entries.Sum(e => e.Amount.Amount);
    public decimal ObligatoryTotal => _entries.Where(e => e.Nature == ExpenseNature.Obligatory).Sum(e => e.Amount.Amount);
    public decimal LeisureTotal => _entries.Where(e => e.Nature == ExpenseNature.Leisure).Sum(e => e.Amount.Amount);

    private MonthlyBudget() { }

    public static Result<MonthlyBudget> Create(Guid userId, DatePeriod period, CurrencyCode homeCurrency, Guid? monthlyIncomeId = null)
    {
        if (userId == Guid.Empty)
            return Result.Failure<MonthlyBudget>(Error.Validation("UserId", "UserId is required."));

        return Result.Success(new MonthlyBudget
        {
            UserId = userId,
            Period = period,
            HomeCurrency = homeCurrency,
            MonthlyIncomeId = monthlyIncomeId
        });
    }

    public Result<ExpenseEntry> AddExpense(
        string description,
        decimal amount,
        CurrencyCode currency,
        ExpenseCategory category,
        ExpenseNature nature,
        DateTime spentDate,
        string? merchant = null,
        string? receiptImageUrl = null,
        bool isAutoCategorised = false,
        string? notes = null)
    {
        if (!Period.Contains(spentDate))
            return Result.Failure<ExpenseEntry>(Error.BusinessRule("WrongPeriod",
                $"Spent date does not fall within period {Period}."));

        var moneyResult = Money.Create(amount, currency);
        if (moneyResult.IsFailure) return Result.Failure<ExpenseEntry>(moneyResult.Error);

        var entryResult = ExpenseEntry.Create(
            description, moneyResult.Value, category, nature,
            spentDate, merchant, receiptImageUrl, isAutoCategorised, notes);

        if (entryResult.IsFailure) return Result.Failure<ExpenseEntry>(entryResult.Error);

        _entries.Add(entryResult.Value);
        Raise(new ExpenseRecordedEvent(
            entryResult.Value.Id, Id, amount, currency.Value, category.ToString(), DateTime.UtcNow));
        Touch();

        return Result.Success(entryResult.Value);
    }

    public Result RecategoriseEntry(Guid entryId, ExpenseCategory newCategory)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is null) return Result.Failure(Error.NotFound("ExpenseEntry", entryId));
        entry.UpdateCategory(newCategory, false);
        return Result.Success();
    }
}
