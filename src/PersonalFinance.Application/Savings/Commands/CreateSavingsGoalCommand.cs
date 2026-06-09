using FluentValidation;
using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Savings.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Savings.Entities;
using PersonalFinance.Domain.Savings.Enums;

namespace PersonalFinance.Application.Savings.Commands;

public sealed record CreateSavingsGoalCommand(
    Guid UserId,
    string Name,
    SavingsGoalType GoalType,
    decimal TargetAmount,
    string Currency,
    DateTime? TargetDate) : IRequest<Result<SavingsGoalDto>>;

public sealed class CreateSavingsGoalCommandValidator : AbstractValidator<CreateSavingsGoalCommand>
{
    public CreateSavingsGoalCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
    }
}

public sealed class CreateSavingsGoalCommandHandler(
    ISavingsGoalRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSavingsGoalCommand, Result<SavingsGoalDto>>
{
    public async Task<Result<SavingsGoalDto>> Handle(CreateSavingsGoalCommand req, CancellationToken ct)
    {
        var currencyResult = CurrencyCode.Create(req.Currency);
        if (currencyResult.IsFailure) return Result.Failure<SavingsGoalDto>(currencyResult.Error);

        var createResult = SavingsGoal.Create(
            req.UserId, req.Name, req.GoalType,
            req.TargetAmount, currencyResult.Value, req.TargetDate);

        if (createResult.IsFailure) return Result.Failure<SavingsGoalDto>(createResult.Error);

        await repository.AddAsync(createResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(createResult.Value.ToDto());
    }
}
