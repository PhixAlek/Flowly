using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Income.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Errors;

namespace PersonalFinance.Application.Income.Queries;

public sealed record GetMonthlyIncomeQuery(Guid UserId, int Year, int Month)
    : IRequest<Result<MonthlyIncomeDto>>;

public sealed class GetMonthlyIncomeQueryHandler(IMonthlyIncomeRepository repository)
    : IRequestHandler<GetMonthlyIncomeQuery, Result<MonthlyIncomeDto>>
{
    public async Task<Result<MonthlyIncomeDto>> Handle(GetMonthlyIncomeQuery req, CancellationToken ct)
    {
        var periodResult = DatePeriod.Create(req.Year, req.Month);
        if (periodResult.IsFailure) return Result.Failure<MonthlyIncomeDto>(periodResult.Error);

        var monthly = await repository.GetByUserAndPeriodAsync(req.UserId, periodResult.Value, ct);
        if (monthly is null) return Result.Failure<MonthlyIncomeDto>(IncomeErrors.MonthlyIncomeNotFound);

        return Result.Success(monthly.ToDto());
    }
}
