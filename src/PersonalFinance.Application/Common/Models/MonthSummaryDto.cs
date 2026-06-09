namespace PersonalFinance.Application.Common.Models;

/// <summary>Top-level monthly financial snapshot — "money in vs money out".</summary>
public sealed record MonthSummaryDto(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal TotalSaved,
    decimal NetBalance,
    string HomeCurrency,
    IEnumerable<CategoryBreakdownDto> ExpenseBreakdown);

public sealed record CategoryBreakdownDto(string Category, decimal Amount, decimal PercentOfTotal);
