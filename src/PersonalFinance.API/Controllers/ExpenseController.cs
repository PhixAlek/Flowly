using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Application.Expense.Commands;
using PersonalFinance.Application.Expense.Queries;

namespace PersonalFinance.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/expense")]
[Authorize]
public sealed class ExpenseController(IMediator mediator) : ControllerBase
{
    /// <summary>Get monthly budget by user + period.</summary>
    [HttpGet("monthly/{userId:guid}/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMonthly(
        Guid userId, int year, int month, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMonthlyBudgetQuery(userId, year, month), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Description);
    }

    /// <summary>
    /// Add an expense entry with category, nature (obligatory/leisure) and optional receipt URL.
    /// Supports auto-categorisation flag for the screenshot-upload feature.
    /// </summary>
    [HttpPost("monthly/{monthlyBudgetId:guid}/entries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddExpense(
        Guid monthlyBudgetId,
        [FromBody] AddExpenseCommand cmd,
        CancellationToken ct)
    {
        if (cmd.MonthlyBudgetId != monthlyBudgetId)
            return BadRequest("Route id and body id mismatch.");

        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMonthly), result.Value)
            : Problem(result.Error.Description, statusCode: StatusCodes.Status422UnprocessableEntity);
    }
}
