using FlowFi.API.Extensions;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
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
        return (await _sender.Send(cmd, ct)).ToCreatedResult(this);
    }
}
