using FluentValidation;
using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Income.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Enums;
using PersonalFinance.Domain.Income.Errors;

namespace PersonalFinance.Application.Income.Commands;

/// <summary>
/// Adds a new income entry to an existing monthly income.
/// Rule I1: supports multiple entries (salary + bonus + freelance in same month).
/// </summary>
public sealed record AddIncomeEntryCommand(
    Guid MonthlyIncomeId,
    string Source,
    IncomeType Type,
    ContractType ContractType,
    decimal GrossAmount,
    decimal DeductionRatePercent,
    string Currency,
    DateTime ReceivedDate,
    string? Notes) : IRequest<Result<IncomeEntryDto>>;

public sealed class AddIncomeEntryCommandValidator : AbstractValidator<AddIncomeEntryCommand>
{
    public AddIncomeEntryCommandValidator()
    {
        RuleFor(x => x.MonthlyIncomeId).NotEmpty();
        RuleFor(x => x.Source).NotEmpty().MaximumLength(200);
        RuleFor(x => x.GrossAmount).GreaterThan(0).WithMessage("Income amount must be positive.");
        RuleFor(x => x.DeductionRatePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.ReceivedDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow.AddDays(1));
    }
}

public sealed class AddIncomeEntryCommandHandler(
    IMonthlyIncomeRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddIncomeEntryCommand, Result<IncomeEntryDto>>
{
    public async Task<Result<IncomeEntryDto>> Handle(AddIncomeEntryCommand req, CancellationToken ct)
    {
        var monthly = await repository.GetByIdAsync(req.MonthlyIncomeId, ct);
        if (monthly is null) return Result.Failure<IncomeEntryDto>(IncomeErrors.NotFound(req.MonthlyIncomeId));

        var currencyResult = CurrencyCode.Create(req.Currency);
        if (currencyResult.IsFailure) return Result.Failure<IncomeEntryDto>(currencyResult.Error);

        var result = monthly.AddEntry(
            req.Source, req.Type, req.ContractType,
            req.GrossAmount, req.DeductionRatePercent,
            currencyResult.Value, req.ReceivedDate, req.Notes);

        if (result.IsFailure) return Result.Failure<IncomeEntryDto>(result.Error);

        repository.Update(monthly);
        await unitOfWork.SaveChangesAsync(ct);

        return Result.Success(result.Value.ToDto());
    }
}
