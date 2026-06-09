using FluentAssertions;
using Moq;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Income.Commands;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Income.Enums;
using Xunit;

namespace PersonalFinance.UnitTests.Application;

public sealed class AddIncomeEntryHandlerTests
{
    private readonly Mock<IMonthlyIncomeRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddEntryAndReturnDto()
    {
        // Arrange
        var period = DatePeriod.Create(2026, 2).Value;
        var monthly = MonthlyIncome.Create(Guid.NewGuid(), period, CurrencyCode.COP).Value;

        _repoMock.Setup(r => r.GetByIdAsync(monthly.Id, default))
                 .ReturnsAsync(monthly);

        var handler = new AddIncomeEntryCommandHandler(_repoMock.Object, _uowMock.Object);

        var cmd = new AddIncomeEntryCommand(
            MonthlyIncomeId: monthly.Id,
            Source: "Main Salary",
            Type: IncomeType.Salary,
            ContractType: ContractType.Indefinite,
            GrossAmount: 4_500_000,
            DeductionRatePercent: 10,
            Currency: "COP",
            ReceivedDate: new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc),
            Notes: null);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Source.Should().Be("Main Salary");
        result.Value.GrossAmount.Should().Be(4_500_000);
        result.Value.NetAmount.Should().Be(4_050_000); // 10% deducted
        _repoMock.Verify(r => r.Update(monthly), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_MonthlyIncomeNotFound_ShouldReturnFailure()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
                 .ReturnsAsync((MonthlyIncome?)null);

        var handler = new AddIncomeEntryCommandHandler(_repoMock.Object, _uowMock.Object);

        var cmd = new AddIncomeEntryCommand(
            Guid.NewGuid(), "Salary", IncomeType.Salary, ContractType.Indefinite,
            3_000_000, 0, "COP", DateTime.UtcNow, null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("NotFound");
    }
}
