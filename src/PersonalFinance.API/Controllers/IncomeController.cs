using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Application.Income.Commands;
using PersonalFinance.Application.Income.Queries;

namespace PersonalFinance.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/income")]
[Authorize]
public sealed class IncomeController(IMediator mediator) : ControllerBase
{
    /// <summary>Create a monthly income record for a given period.</summary>
    [HttpPost("monthly")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMonthly(
        [FromBody] CreateMonthlyIncomeCommand cmd,
        CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMonthly),
                new { userId = result.Value.UserId, year = result.Value.Year, month = result.Value.Month },
                result.Value)
            : Problem(result.Error.Description, statusCode: StatusCodes.Status409Conflict);
    }

    /// <summary>Get monthly income by user + period.</summary>
    [HttpGet("monthly/{userId:guid}/{year:int}/{month:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMonthly(
        Guid userId, int year, int month, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMonthlyIncomeQuery(userId, year, month), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Description);
    }

    /// <summary>
    /// Add an income entry (salary, bonus, freelance, prima, etc.).
    /// Rule I1: multiple entries supported per month.
    /// </summary>
    [HttpPost("monthly/{monthlyIncomeId:guid}/entries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AddEntry(
        Guid monthlyIncomeId,
        [FromBody] AddIncomeEntryCommand cmd,
        CancellationToken ct)
    {
        if (cmd.MonthlyIncomeId != monthlyIncomeId)
            return BadRequest("Route id and body id mismatch.");

        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMonthly), result.Value)
            : Problem(result.Error.Description, statusCode: StatusCodes.Status422UnprocessableEntity);
    }

    /// <summary>
    /// Convert an income entry to the user's home currency.
    /// Rule I6: freelancers paid in USD/EUR → see COP equivalent.
    /// </summary>
    [HttpPost("monthly/{monthlyIncomeId:guid}/entries/{entryId:guid}/convert")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConvertCurrency(
        Guid monthlyIncomeId, Guid entryId,
        [FromQuery] string targetCurrency,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new ConvertIncomeCurrencyCommand(monthlyIncomeId, entryId, targetCurrency), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error.Description);
    }
}
