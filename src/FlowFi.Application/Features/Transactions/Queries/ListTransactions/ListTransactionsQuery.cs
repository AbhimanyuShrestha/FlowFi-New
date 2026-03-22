using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Common;
using FlowFi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Transactions.Queries.ListTransactions;

public record ListTransactionsQuery(
    Guid UserId,
    int Limit = 20,
    DateTime? Cursor = null,
    Guid? CategoryId = null,
    TransactionType? Type = null,
    DateTime? From = null,
    DateTime? To = null
) : IRequest<Result<PagedResult<TransactionDto>>>;

public record PagedResult<T>(IReadOnlyList<T> Items, string? Cursor, bool HasMore);

public class ListTransactionsQueryHandler
    : IRequestHandler<ListTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    private readonly IAppDbContext _db;

    public ListTransactionsQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<PagedResult<TransactionDto>>> Handle(
        ListTransactionsQuery request, CancellationToken ct)
    {
        var query = _db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == request.UserId)
            .AsQueryable();

        if (request.CategoryId.HasValue) query = query.Where(t => t.CategoryId == request.CategoryId);
        if (request.Type.HasValue)       query = query.Where(t => t.Type == request.Type);
        if (request.From.HasValue)       query = query.Where(t => t.OccurredAt >= request.From);
        if (request.To.HasValue)         query = query.Where(t => t.OccurredAt <= request.To);
        if (request.Cursor.HasValue)     query = query.Where(t => t.OccurredAt < request.Cursor);

        var rows = await query
            .OrderByDescending(t => t.OccurredAt)
            .Take(request.Limit + 1)  // +1 to detect hasMore
            .ToListAsync(ct);

        var hasMore = rows.Count > request.Limit;
        var items   = hasMore ? rows.Take(request.Limit).ToList() : rows;
        var cursor  = hasMore ? items.Last().OccurredAt.ToString("o") : null;

        var dtos = items.Select(t => new TransactionDto(
            t.Id, t.UserId, t.Amount, t.Type.ToString(),
            t.Description, t.Note, t.CategoryId,
            t.Category is null ? null : new CategoryDto(t.Category.Id, t.Category.Name, t.Category.Icon, t.Category.Color),
            t.OccurredAt, t.CreatedAt
        )).ToList();

        return Result<PagedResult<TransactionDto>>.Success(new PagedResult<TransactionDto>(dtos, cursor, hasMore));
    }
}
