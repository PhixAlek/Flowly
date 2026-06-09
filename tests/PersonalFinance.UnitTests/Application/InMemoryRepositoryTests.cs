using FluentAssertions;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Income.Enums;
using PersonalFinance.Infrastructure.Persistence.InMemory;
using System.Collections.Concurrent;
using Xunit;

namespace PersonalFinance.UnitTests.Application;

/// <summary>
/// Verifies that the in-memory repositories correctly implement the
/// IRepository contracts — these are the default backing stores for the
/// boilerplate before a real database is connected.
/// </summary>
public sealed class InMemoryRepositoryTests
{
    private static MonthlyIncome CreateIncome(int year = 2026, int month = 2)
    {
        var period = DatePeriod.Create(year, month).Value;
        return MonthlyIncome.Create(Guid.NewGuid(), period, CurrencyCode.COP).Value;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldReturnSameEntity()
    {
        var store = new ConcurrentDictionary<Guid, MonthlyIncome>();
        var repo = new InMemoryMonthlyIncomeRepository(store);
        var income = CreateIncome();

        await repo.AddAsync(income);
        var retrieved = await repo.GetByIdAsync(income.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(income.Id);
    }

    [Fact]
    public async Task Remove_ShouldDeleteFromStore()
    {
        var store = new ConcurrentDictionary<Guid, MonthlyIncome>();
        var repo = new InMemoryMonthlyIncomeRepository(store);
        var income = CreateIncome();

        await repo.AddAsync(income);
        repo.Remove(income);
        var retrieved = await repo.GetByIdAsync(income.Id);

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task ExistsForPeriodAsync_WhenExists_ShouldReturnTrue()
    {
        var store = new ConcurrentDictionary<Guid, MonthlyIncome>();
        var repo = new InMemoryMonthlyIncomeRepository(store);
        var income = CreateIncome(2026, 3);

        await repo.AddAsync(income);

        var period = DatePeriod.Create(2026, 3).Value;
        var exists = await repo.ExistsForPeriodAsync(income.UserId, period);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUserAndPeriodAsync_WhenNotFound_ShouldReturnNull()
    {
        var store = new ConcurrentDictionary<Guid, MonthlyIncome>();
        var repo = new InMemoryMonthlyIncomeRepository(store);
        var period = DatePeriod.Create(2026, 1).Value;

        var result = await repo.GetByUserAndPeriodAsync(Guid.NewGuid(), period);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InMemoryUnitOfWork_SaveChanges_ShouldReturnZeroWithoutThrowing()
    {
        var uow = new InMemoryUnitOfWork();
        var result = await uow.SaveChangesAsync();
        result.Should().Be(0);
    }
}
