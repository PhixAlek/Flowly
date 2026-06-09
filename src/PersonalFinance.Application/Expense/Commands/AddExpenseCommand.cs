using FluentValidation;
using MediatR;
using PersonalFinance.Application.Common.Interfaces;
using PersonalFinance.Application.Expense.DTOs;
using PersonalFinance.Domain.Common.Primitives;
using PersonalFinance.Domain.Common.ValueObjects;
using PersonalFinance.Domain.Expense.Enums;
using PersonalFinance.Domain.Expense.Errors;

namespace PersonalFinance.Application.Expense.Commands;

public sealed record AddExpenseCommand(
    Guid MonthlyBudgetId,
    string Description,
    decimal Amount,
    string Currency,
    ExpenseCategory Category,
    ExpenseNature Nature,
    DateTime SpentDate,
    string? Merchant,
    string? ReceiptImageUrl,
    bool IsAutoCategorised,
    string? Notes) : IRequest<Result<ExpenseEntryDto>>;

public sealed class AddExpenseCommandValidator : AbstractValidator<AddExpenseCommand>
{
    public AddExpenseCommandValidator()
    {
        RuleFor(x => x.MonthlyBudgetId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.SpentDate).NotEmpty();
    }
}

public sealed class AddExpenseCommandHandler(
    IMonthlyBudgetRepository repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddExpenseCommand, Result<ExpenseEntryDto>>
{
    public async Task<Result<ExpenseEntryDto>> Handle(AddExpenseCommand req, CancellationToken ct)
    {
        var budget = await repository.GetByIdAsync(req.MonthlyBudgetId, ct);
        if (budget is null)
            return Result.Failure<ExpenseEntryDto>(Error.NotFound("MonthlyBudget", req.MonthlyBudgetId));

        var currencyResult = CurrencyCode.Create(req.Currency);
        if (currencyResult.IsFailure) return Result.Failure<ExpenseEntryDto>(currencyResult.Error);

        var result = budget.AddExpense(
            req.Description, req.Amount, currencyResult.Value,
            req.Category, req.Nature, req.SpentDate,
            req.Merchant, req.ReceiptImageUrl, req.IsAutoCategorised, req.Notes);

        if (result.IsFailure) return Result.Failure<ExpenseEntryDto>(result.Error);

        repository.Update(budget);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(result.Value.ToDto());
    }
}
