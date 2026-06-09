using PersonalFinance.Domain.Income.Entities;

namespace PersonalFinance.Application.Income.DTOs;

internal static class IncomeMappings
{
    public static IncomeEntryDto ToDto(this IncomeEntry e) => new(
        e.Id, e.Source, e.Type, e.Nature, e.ContractType,
        e.TaxInfo.GrossAmount, e.TaxInfo.DeductionRatePercent,
        e.TaxInfo.NetAmount, e.TaxInfo.DeductionAmount,
        e.OriginalCurrency.Value,
        e.ConvertedAmount?.Amount, e.ConvertedAmount?.Currency.Value,
        e.ExchangeRateUsed,
        e.ReceivedDate, e.Notes);

    public static MonthlyIncomeDto ToDto(this MonthlyIncome m) => new(
        m.Id, m.UserId, m.Period.Year, m.Period.Month,
        m.HomeCurrency.Value,
        m.TotalGross, m.TotalNet, m.TotalDeductions, m.TotalNetInHomeCurrency,
        m.Entries.Select(e => e.ToDto()),
        m.CreatedAtUtc);
}
