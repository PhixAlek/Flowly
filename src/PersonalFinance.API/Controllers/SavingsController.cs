using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Application.Savings.Commands;

namespace PersonalFinance.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/savings")]
[Authorize]
public sealed class SavingsController(IMediator mediator) : ControllerBase
{
    /// <summary>Create a savings goal (emergency fund, trip, etc.).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSavingsGoalCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Deposit), new { goalId = result.Value.Id }, result.Value)
            : Problem(result.Error.Description, statusCode: StatusCodes.Status422UnprocessableEntity);
    }

    /// <summary>Deposit an amount into a savings goal.</summary>
    [HttpPost("{goalId:guid}/deposit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deposit(
        Guid goalId, [FromBody] decimal amount, CancellationToken ct)
    {
        var result = await mediator.Send(new DepositToSavingsCommand(goalId, amount), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error.Description);
    }
}
