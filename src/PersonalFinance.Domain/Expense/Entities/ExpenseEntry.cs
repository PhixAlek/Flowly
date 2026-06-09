using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Expense.Enums;
using PersonalFinance.Domain.Expense.Errors;

namespace PersonalFinance.Domain.Expense.Entities;

/// <summary>A single expense. Idea: screenshot + auto-categorise is supported via Description/Metadata fields.</summary>
public sealed class ExpenseEntry : Entity
{
    public string Description { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public ExpenseCategory Category { get; private set; }
    public ExpenseNature Nature { get; private set; }
    public DateTime SpentDate { get; private set; }
    public string? Merchant { get; private set; }
    public string? ReceiptImageUrl { get; private set; }  // for screenshot upload feature
    public bool IsAutoCategorised { get; private set; }
    public string? Notes { get; private set; }

    private ExpenseEntry() { }

    internal static Result<ExpenseEntry> Create(
        string description,
        Money amount,
        ExpenseCategory category,
        ExpenseNature nature,
        DateTime spentDate,
        string? merchant,
        string? receiptImageUrl,
        bool isAutoCategorised,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<ExpenseEntry>(
                Error.Validation("Description", "Expense description is required."));
        if (spentDate > DateTime.UtcNow.AddDays(1))
            return Result.Failure<ExpenseEntry>(ExpenseErrors.FutureDateNotAllowed);

        return Result.Success(new ExpenseEntry
        {
            Description = description.Trim(),
            Amount = amount,
            Category = category,
            Nature = nature,
            SpentDate = spentDate,
            Merchant = merchant,
            ReceiptImageUrl = receiptImageUrl,
            IsAutoCategorised = isAutoCategorised,
            Notes = notes
        });
    }

    internal void UpdateCategory(ExpenseCategory category, bool auto)
    {
        Category = category;
        IsAutoCategorised = auto;
        Touch();
    }
}
