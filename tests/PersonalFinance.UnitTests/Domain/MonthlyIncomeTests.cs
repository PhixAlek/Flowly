using FluentAssertions;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Income.Enums;
using PersonalFinance.Domain.Income.Events;
using Xunit;

namespace PersonalFinance.UnitTests.Domain;

public sealed class MonthlyIncomeTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DatePeriod Feb2026 = DatePeriod.Create(2026, 2).Value;
    private static readonly CurrencyCode COP = CurrencyCode.COP;

    private static MonthlyIncome CreateIncome() =>
        MonthlyIncome.Create(UserId, Feb2026, COP).Value;

    [Fact]
    public void Create_ValidArgs_ShouldSucceed()
    {
        var income = CreateIncome();
        income.UserId.Should().Be(UserId);
        income.Period.Year.Should().Be(2026);
        income.Period.Month.Should().Be(2);
    }

    /// <summary>Rule I1: multiple incomes in one month.</summary>
    [Fact]
    public void AddEntry_MultipleSources_ShouldAllBeRecorded()
    {
        var income = CreateIncome();
        var date = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc);

        income.AddEntry("Salary", IncomeType.Salary, ContractType.Indefinite,
            5_000_000, 10, COP, date);
        income.AddEntry("Freelance Project", IncomeType.Freelance, ContractType.Freelance,
            800_000, 0, COP, date);
        income.AddEntry("End-of-year bonus", IncomeType.Prima, ContractType.Indefinite,
            500_000, 10, COP, date);

        income.Entries.Should().HaveCount(3);
        income.TotalGross.Should().Be(6_300_000);
    }

    [Theory]
    [InlineData(IncomeType.Salary)]
    [InlineData(IncomeType.Bonus)]
    [InlineData(IncomeType.Freelance)]
    [InlineData(IncomeType.Prima)]
    [InlineData(IncomeType.InternationalFreelance)]
    public void AddEntry_WorkBasedIncome_ShouldBeActive(IncomeType type)
    {
        var income = CreateIncome();

        var result = income.AddEntry(
            type.ToString(), type, ContractType.Freelance,
            1_000_000, 0, COP, new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc));

        result.Value.Nature.Should().Be(IncomeNature.Active);
    }

    [Theory]
    [InlineData(IncomeType.Rental)]
    [InlineData(IncomeType.Dividend)]
    [InlineData(IncomeType.Interest)]
    public void AddEntry_AssetBasedIncome_ShouldBePassive(IncomeType type)
    {
        var income = CreateIncome();

        var result = income.AddEntry(
            type.ToString(), type, ContractType.None,
            1_000_000, 0, COP, new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc));

        result.Value.Nature.Should().Be(IncomeNature.Passive);
    }

    /// <summary>Rule I3: negative amount is rejected at the Money level.</summary>
    [Fact]
    public void AddEntry_NegativeAmount_ShouldFail()
    {
        var income = CreateIncome();
        var result = income.AddEntry("Bad", IncomeType.Other, ContractType.None,
            -500, 0, COP, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        result.IsFailure.Should().BeTrue();
    }

    /// <summary>Rule I2: net = gross - deductions.</summary>
    [Fact]
    public void TotalNet_ShouldReflectDeductions()
    {
        var income = CreateIncome();
        income.AddEntry("Salary", IncomeType.Salary, ContractType.Indefinite,
            4_000_000, 10, COP, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        income.TotalNet.Should().Be(3_600_000); // 10% deducted
        income.TotalDeductions.Should().Be(400_000);
    }

    [Fact]
    public void AddEntry_OutsidePeriod_ShouldFail()
    {
        var income = CreateIncome();
        var wrongDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc); // March, not Feb
        var result = income.AddEntry("Salary", IncomeType.Salary, ContractType.Indefinite,
            1000, 0, COP, wrongDate);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("WrongPeriod");
    }

    [Fact]
    public void AddEntry_ShouldRaiseDomainEvent()
    {
        var income = CreateIncome();
        income.AddEntry("Salary", IncomeType.Salary, ContractType.Indefinite,
            3_000_000, 0, COP, new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc));
        income.DomainEvents.Should().ContainSingle(e => e is IncomeRecordedEvent);
    }
}
