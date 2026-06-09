using InvestmentEntity = PersonalFinance.Domain.Investment.Entities.Investment;

namespace PersonalFinance.Application.Investment.DTOs;

internal static class InvestmentMappings
{
    public static InvestmentDto ToDto(this InvestmentEntity i) => new(
        i.Id, i.UserId, i.Name, i.Type, i.Status,
        i.Principal.Amount, i.Principal.Currency.Value,
        i.AnnualRatePercent, i.EstimatedYield,
        i.StartDate, i.MaturityDate,
        i.MaturityValue?.Amount,
        i.Institution, i.Notes, i.CreatedAtUtc);
}
