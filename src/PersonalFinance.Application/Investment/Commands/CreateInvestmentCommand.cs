using FluentValidation;
using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Investment.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Investment.Enums;
using InvestmentEntity = PersonalFinance.Domain.Investment.Entities.Investment;

namespace PersonalFinance.Application.Investment.Commands;

public sealed record CreateInvestmentCommand(
    Guid UserId,
    string Name,
    InvestmentType Type,
    decimal Principal,
    string Currency,
    decimal AnnualRatePercent,
    DateTime StartDate,
    DateTime? MaturityDate,
    string? Institution,
    string? Notes) : IRequest<Result<InvestmentDto>>;

public sealed class CreateInvestmentCommandValidator : AbstractValidator<CreateInvestmentCommand>
{
    public CreateInvestmentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Principal).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.AnnualRatePercent).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaturityDate).GreaterThan(x => x.StartDate).When(x => x.MaturityDate.HasValue);
    }
}

public sealed class CreateInvestmentCommandHandler(IInvestmentRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateInvestmentCommand, Result<InvestmentDto>>
{
    public async Task<Result<InvestmentDto>> Handle(CreateInvestmentCommand req, CancellationToken ct)
    {
        var currencyResult = CurrencyCode.Create(req.Currency);
        if (currencyResult.IsFailure) return Result.Failure<InvestmentDto>(currencyResult.Error);

        var createResult = InvestmentEntity.Create(
            req.UserId, req.Name, req.Type,
            req.Principal, currencyResult.Value,
            req.AnnualRatePercent, req.StartDate,
            req.MaturityDate, req.Institution, req.Notes);

        if (createResult.IsFailure) return Result.Failure<InvestmentDto>(createResult.Error);

        await repository.AddAsync(createResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(createResult.Value.ToDto());
    }
}
