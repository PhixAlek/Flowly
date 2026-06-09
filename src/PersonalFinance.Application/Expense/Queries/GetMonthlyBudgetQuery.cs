using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Expense.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;

namespace PersonalFinance.Application.Expense.Queries;

public sealed record GetMonthlyBudgetQuery(Guid UserId, int Year, int Month)
    : IRequest<Result<MonthlyBudgetDto>>;

public sealed class GetMonthlyBudgetQueryHandler(IMonthlyBudgetRepository repository)
    : IRequestHandler<GetMonthlyBudgetQuery, Result<MonthlyBudgetDto>>
{
    public async Task<Result<MonthlyBudgetDto>> Handle(GetMonthlyBudgetQuery req, CancellationToken ct)
    {
        var periodResult = DatePeriod.Create(req.Year, req.Month);
        if (periodResult.IsFailure) return Result.Failure<MonthlyBudgetDto>(periodResult.Error);

        var budget = await repository.GetByUserAndPeriodAsync(req.UserId, periodResult.Value, ct);
        if (budget is null)
            return Result.Failure<MonthlyBudgetDto>(Error.NotFound("MonthlyBudget", req.UserId));

        return Result.Success(budget.ToDto());
    }
}
