using PersonalFinance.Domain.Income.Enums;

namespace PersonalFinance.Application.Income.DTOs;

public sealed record IncomeEntryDto(
    Guid Id,
    string Source,
    IncomeType Type,
    IncomeNature Nature,
    ContractType ContractType,
    decimal GrossAmount,
    decimal DeductionRatePercent,
    decimal NetAmount,
    decimal DeductionAmount,
    string OriginalCurrency,
    decimal? ConvertedAmount,
    string? ConvertedCurrency,
    decimal? ExchangeRateUsed,
    DateTime ReceivedDate,
    string? Notes);

public sealed record MonthlyIncomeDto(
    Guid Id,
    Guid UserId,
    int Year,
    int Month,
    string HomeCurrency,
    decimal TotalGross,
    decimal TotalNet,
    decimal TotalDeductions,
    decimal TotalNetInHomeCurrency,
    IEnumerable<IncomeEntryDto> Entries,
    DateTime CreatedAtUtc);
