using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Common.ValueObjects;

/// <summary>
/// Encapsulates gross/net relationship.
/// Rule I2: "How do I calculate net income over gross?"
/// Net = Gross - Deductions. DeductionRate is a percentage (0-100).
/// </summary>
public sealed class TaxInfo : ValueObject
{
    public decimal GrossAmount { get; }
    public decimal DeductionRatePercent { get; }
    public decimal NetAmount => Math.Round(GrossAmount * (1 - DeductionRatePercent / 100m), 2);
    public decimal DeductionAmount => GrossAmount - NetAmount;

    private TaxInfo(decimal gross, decimal rate) { GrossAmount = gross; DeductionRatePercent = rate; }

    public static Result<TaxInfo> Create(decimal gross, decimal deductionRatePercent)
    {
        if (gross < 0)
            return Result.Failure<TaxInfo>(Error.Validation("Gross", "Gross amount cannot be negative."));
        if (deductionRatePercent is < 0 or > 100)
            return Result.Failure<TaxInfo>(Error.Validation("DeductionRate", "Deduction rate must be between 0 and 100."));
        return Result.Success(new TaxInfo(gross, deductionRatePercent));
    }

    protected override IEnumerable<object?> GetComponents()
    {
        yield return GrossAmount;
        yield return DeductionRatePercent;
    }
}
