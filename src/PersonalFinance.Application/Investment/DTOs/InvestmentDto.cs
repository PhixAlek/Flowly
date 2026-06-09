using PersonalFinance.Domain.Investment.Enums;

namespace PersonalFinance.Application.Investment.DTOs;

public sealed record InvestmentDto(
    Guid Id,
    Guid UserId,
    string Name,
    InvestmentType Type,
    InvestmentStatus Status,
    decimal Principal,
    string Currency,
    decimal AnnualRatePercent,
    decimal EstimatedYield,
    DateTime StartDate,
    DateTime? MaturityDate,
    decimal? MaturityValue,
    string? Institution,
    string? Notes,
    DateTime CreatedAtUtc);
