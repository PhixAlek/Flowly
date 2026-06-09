using FluentValidation;
using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Income.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Entities;
using PersonalFinance.Domain.Income.Errors;

namespace PersonalFinance.Application.Income.Commands;

public sealed record CreateMonthlyIncomeCommand(
    Guid UserId,
    int Year,
    int Month,
    string HomeCurrency) : IRequest<Result<MonthlyIncomeDto>>;

public sealed class CreateMonthlyIncomeCommandValidator : AbstractValidator<CreateMonthlyIncomeCommand>
{
    public CreateMonthlyIncomeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.HomeCurrency).NotEmpty().Length(3);
    }
}

public sealed class CreateMonthlyIncomeCommandHandler(
    IMonthlyIncomeRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMonthlyIncomeCommand, Result<MonthlyIncomeDto>>
{
    public async Task<Result<MonthlyIncomeDto>> Handle(CreateMonthlyIncomeCommand req, CancellationToken ct)
    {
        var periodResult = DatePeriod.Create(req.Year, req.Month);
        if (periodResult.IsFailure) return Result.Failure<MonthlyIncomeDto>(periodResult.Error);

        var currencyResult = CurrencyCode.Create(req.HomeCurrency);
        if (currencyResult.IsFailure) return Result.Failure<MonthlyIncomeDto>(currencyResult.Error);

        if (await repository.ExistsForPeriodAsync(req.UserId, periodResult.Value, ct))
            return Result.Failure<MonthlyIncomeDto>(IncomeErrors.DuplicatePeriod);

        var createResult = MonthlyIncome.Create(req.UserId, periodResult.Value, currencyResult.Value);
        if (createResult.IsFailure) return Result.Failure<MonthlyIncomeDto>(createResult.Error);

        await repository.AddAsync(createResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(createResult.Value.ToDto());
    }
}
