using FlowFi.API.Extensions;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Application.Features.Transactions.Commands.DeleteTransaction;
using FlowFi.Application.Features.Transactions.Commands.UpdateTransaction;
using FlowFi.Application.Features.Transactions.Queries.GetTransactionById;
using FlowFi.Application.Features.Transactions.Queries.ListTransactions;
using FlowFi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FlowFi.API.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ISender _sender;
    public TransactionsController(ISender sender) => _sender = sender;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int limit = 20,
        [FromQuery] DateTime? cursor = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] TransactionType? type = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
        => (await _sender.Send(
            new ListTransactionsQuery(CurrentUserId, limit, cursor, categoryId, type, from, to), ct))
            .ToActionResult(this);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId };
        var result = await _sender.Send(cmd, ct);
        
        if (result.IsSuccess && !result.Value!.IsNew)
            return Ok(new { success = true, data = result.Value });

        return result.ToCreatedResult(this);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => (await _sender.Send(new GetTransactionByIdQuery(CurrentUserId, id), ct))
            .ToActionResult(this);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionCommand command, CancellationToken ct)
    {
        var cmd = command with { UserId = CurrentUserId, TransactionId = id };
        return (await _sender.Send(cmd, ct)).ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => (await _sender.Send(new DeleteTransactionCommand(CurrentUserId, id), ct))
            .ToActionResult(this);
}
