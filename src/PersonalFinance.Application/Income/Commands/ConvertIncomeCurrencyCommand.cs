using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Income.Ports;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Income.Errors;

namespace PersonalFinance.Application.Income.Commands;

/// <summary>
/// Converts a specific income entry to the user's home currency.
/// Rule I6: freelancers paid in USD/EUR can see COP equivalent.
/// </summary>
public sealed record ConvertIncomeCurrencyCommand(
    Guid MonthlyIncomeId,
    Guid EntryId,
    string TargetCurrency) : IRequest<Result>;

public sealed class ConvertIncomeCurrencyCommandHandler(
    IMonthlyIncomeRepository repository,
    ICurrencyConverter converter,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ConvertIncomeCurrencyCommand, Result>
{
    public async Task<Result> Handle(ConvertIncomeCurrencyCommand req, CancellationToken ct)
    {
        var monthly = await repository.GetByIdAsync(req.MonthlyIncomeId, ct);
        if (monthly is null) return Result.Failure(IncomeErrors.NotFound(req.MonthlyIncomeId));

        var entry = monthly.Entries.FirstOrDefault(e => e.Id == req.EntryId);
        if (entry is null) return Result.Failure(Error.NotFound("IncomeEntry", req.EntryId));

        var targetResult = CurrencyCode.Create(req.TargetCurrency);
        if (targetResult.IsFailure) return Result.Failure(targetResult.Error);

        var rate = await converter.GetRateAsync(
            entry.OriginalCurrency.Value, req.TargetCurrency, ct);

        var result = monthly.ConvertEntry(req.EntryId, targetResult.Value, rate);
        if (result.IsFailure) return result;

        repository.Update(monthly);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
