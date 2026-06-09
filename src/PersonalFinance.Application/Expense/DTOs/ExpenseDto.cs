using PersonalFinance.Domain.Expense.Enums;

namespace PersonalFinance.Application.Expense.DTOs;

public sealed record ExpenseEntryDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Currency,
    ExpenseCategory Category,
    ExpenseNature Nature,
    DateTime SpentDate,
    string? Merchant,
    string? ReceiptImageUrl,
    bool IsAutoCategorised,
    string? Notes);

public sealed record MonthlyBudgetDto(
    Guid Id,
    Guid UserId,
    int Year,
    int Month,
    string HomeCurrency,
    Guid? MonthlyIncomeId,
    decimal TotalSpent,
    decimal ObligatoryTotal,
    decimal LeisureTotal,
    IEnumerable<ExpenseEntryDto> Entries,
    DateTime CreatedAtUtc);
