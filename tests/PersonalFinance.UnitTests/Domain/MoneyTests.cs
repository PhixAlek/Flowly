using FluentAssertions;
using PersonalFinance.Domain.Common.ValueObjects;
using Xunit;

namespace PersonalFinance.UnitTests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Create_PositiveAmount_ShouldSucceed()
    {
        var result = Money.Create(1500.50m, CurrencyCode.COP);
        result.IsSuccess.Should().BeTrue();
        result.Value.Amount.Should().Be(1500.50m);
        result.Value.Currency.Should().Be(CurrencyCode.COP);
    }

    [Fact]
    public void Create_NegativeAmount_ShouldFail()
    {
        var result = Money.Create(-1m, CurrencyCode.USD);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NegativeAmount");
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var a = Money.Create(100m, CurrencyCode.USD).Value;
        var b = Money.Create(50m, CurrencyCode.USD).Value;
        var sum = a.Add(b);
        sum.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var cop = Money.Create(100m, CurrencyCode.COP).Value;
        var usd = Money.Create(100m, CurrencyCode.USD).Value;
        var act = () => cop.Add(usd);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_BelowZero_ShouldThrow()
    {
        var a = Money.Create(50m, CurrencyCode.USD).Value;
        var b = Money.Create(100m, CurrencyCode.USD).Value;
        var act = () => a.Subtract(b);
        act.Should().Throw<InvalidOperationException>();
    }
}
