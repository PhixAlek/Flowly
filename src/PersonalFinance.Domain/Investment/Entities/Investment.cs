using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Investment.Enums;
using PersonalFinance.Domain.Investment.Errors;
using PersonalFinance.Domain.Investment.Events;

namespace PersonalFinance.Domain.Investment.Entities;

/// <summary>
/// Aggregate root for an investment position (CDT, stock, etc.).
/// Tracks principal, expected yield and maturity.
/// </summary>
public sealed class Investment : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public InvestmentType Type { get; private set; }
    public InvestmentStatus Status { get; private set; }
    public Money Principal { get; private set; } = null!;
    public decimal AnnualRatePercent { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? MaturityDate { get; private set; }
    public Money? MaturityValue { get; private set; }
    public string? Institution { get; private set; }
    public string? Notes { get; private set; }

    /// <summary>Estimated yield at maturity (simple interest).</summary>
    public decimal EstimatedYield =>
        MaturityDate.HasValue
            ? Math.Round(Principal.Amount * AnnualRatePercent / 100m *
              (decimal)(MaturityDate.Value - StartDate).TotalDays / 365m, 2)
            : 0;

    private Investment() { }

    public static Result<Investment> Create(
        Guid userId, string name, InvestmentType type,
        decimal principal, CurrencyCode currency,
        decimal annualRatePercent, DateTime startDate,
        DateTime? maturityDate = null, string? institution = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Investment>(Error.Validation("Name", "Investment name is required."));
        if (annualRatePercent < 0)
            return Result.Failure<Investment>(Error.Validation("Rate", "Annual rate cannot be negative."));

        var principalResult = Money.Create(principal, currency);
        if (principalResult.IsFailure) return Result.Failure<Investment>(principalResult.Error);

        var inv = new Investment
        {
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Status = InvestmentStatus.Active,
            Principal = principalResult.Value,
            AnnualRatePercent = annualRatePercent,
            StartDate = startDate,
            MaturityDate = maturityDate,
            Institution = institution,
            Notes = notes
        };

        inv.Raise(new InvestmentCreatedEvent(inv.Id, type.ToString(), principal, currency.Value, DateTime.UtcNow));
        return Result.Success(inv);
    }

    public Result Liquidate(decimal finalAmount)
    {
        if (Status == InvestmentStatus.Liquidated)
            return Result.Failure(InvestmentErrors.AlreadyLiquidated);

        var matResult = Money.Create(finalAmount, Principal.Currency);
        if (matResult.IsFailure) return Result.Failure(matResult.Error);

        MaturityValue = matResult.Value;
        Status = InvestmentStatus.Liquidated;
        Touch();
        return Result.Success();
    }
}
