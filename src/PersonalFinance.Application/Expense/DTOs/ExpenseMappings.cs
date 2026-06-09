using PersonalFinance.Domain.Expense.Entities;

namespace PersonalFinance.Application.Expense.DTOs;

internal static class ExpenseMappings
{
    public static ExpenseEntryDto ToDto(this ExpenseEntry e) => new(
        e.Id, e.Description, e.Amount.Amount, e.Amount.Currency.Value,
        e.Category, e.Nature, e.SpentDate,
        e.Merchant, e.ReceiptImageUrl, e.IsAutoCategorised, e.Notes);

    public static MonthlyBudgetDto ToDto(this MonthlyBudget b) => new(
        b.Id, b.UserId, b.Period.Year, b.Period.Month,
        b.HomeCurrency.Value, b.MonthlyIncomeId,
        b.TotalSpent, b.ObligatoryTotal, b.LeisureTotal,
        b.Entries.Select(e => e.ToDto()),
        b.CreatedAtUtc);
}
