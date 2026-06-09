using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Application.Investment.Commands;

namespace PersonalFinance.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/investments")]
[Authorize]
public sealed class InvestmentController(IMediator mediator) : ControllerBase
{
    /// <summary>Record a new investment (CDT, stock, fund, etc.).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvestmentCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Create), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error.Description, statusCode: StatusCodes.Status422UnprocessableEntity);
    }
}
