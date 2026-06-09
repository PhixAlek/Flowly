using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Savings.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Savings.Errors;

namespace PersonalFinance.Application.Savings.Commands;

public sealed record DepositToSavingsCommand(Guid GoalId, decimal Amount) : IRequest<Result<SavingsGoalDto>>;

public sealed class DepositToSavingsCommandHandler(ISavingsGoalRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<DepositToSavingsCommand, Result<SavingsGoalDto>>
{
    public async Task<Result<SavingsGoalDto>> Handle(DepositToSavingsCommand req, CancellationToken ct)
    {
        var goal = await repository.GetByIdAsync(req.GoalId, ct);
        if (goal is null) return Result.Failure<SavingsGoalDto>(SavingsErrors.NotFound(req.GoalId));

        var result = goal.Deposit(req.Amount);
        if (result.IsFailure) return Result.Failure<SavingsGoalDto>(result.Error);

        repository.Update(goal);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(goal.ToDto());
    }
}
