using FluentAssertions;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Savings.Entities;
using PersonalFinance.Domain.Savings.Enums;
using Xunit;

namespace PersonalFinance.UnitTests.Domain;

public sealed class SavingsGoalTests
{
    private static SavingsGoal CreateEmergencyFund() =>
        SavingsGoal.Create(Guid.NewGuid(), "Emergency Fund", SavingsGoalType.Emergency,
            10_000_000, CurrencyCode.COP).Value;

    [Fact]
    public void Deposit_ValidAmount_ShouldIncreaseBalance()
    {
        var goal = CreateEmergencyFund();
        goal.Deposit(3_000_000);
        goal.CurrentAmount.Amount.Should().Be(3_000_000);
        goal.ProgressPercent.Should().Be(30);
    }

    [Fact]
    public void Deposit_ReachingTarget_ShouldMarkAchieved()
    {
        var goal = CreateEmergencyFund();
        goal.Deposit(10_000_000);
        goal.IsAchieved.Should().BeTrue();
    }

    [Fact]
    public void Withdraw_MoreThanBalance_ShouldFail()
    {
        var goal = CreateEmergencyFund();
        goal.Deposit(1_000_000);
        var result = goal.Withdraw(5_000_000);
        result.IsFailure.Should().BeTrue();
    }
}
