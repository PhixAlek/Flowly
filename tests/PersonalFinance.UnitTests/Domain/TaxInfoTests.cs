using FluentAssertions;
using PersonalFinance.Domain.Common.ValueObjects;
using Xunit;

namespace PersonalFinance.UnitTests.Domain;

/// <summary>Tests for Rule I2: How do I calculate net income over gross?</summary>
public sealed class TaxInfoTests
{
    [Theory]
    [InlineData(5_000_000, 10, 4_500_000, 500_000)]  // 10% deduction
    [InlineData(3_000_000, 0, 3_000_000, 0)]           // no deduction
    [InlineData(2_000_000, 25, 1_500_000, 500_000)]    // 25% deduction
    public void Create_ValidInputs_ShouldCalculateNetCorrectly(
        decimal gross, decimal rate, decimal expectedNet, decimal expectedDeduction)
    {
        var result = TaxInfo.Create(gross, rate);
        result.IsSuccess.Should().BeTrue();
        result.Value.NetAmount.Should().Be(expectedNet);
        result.Value.DeductionAmount.Should().Be(expectedDeduction);
    }

    [Fact]
    public void Create_NegativeGross_ShouldFail()
    {
        var result = TaxInfo.Create(-100, 10);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_RateAbove100_ShouldFail()
    {
        var result = TaxInfo.Create(1000, 101);
        result.IsFailure.Should().BeTrue();
    }
}
