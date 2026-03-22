using FlowFi.Application.Common;
using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using FlowFi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Transactions.Commands.CreateTransaction;

public record CreateTransactionCommand(
    Guid UserId,
    decimal Amount,
    TransactionType Type,
    string? Description,
    string? Note,
    Guid? CategoryId,
    DateTime? OccurredAt,
    string? IdempotencyKey
) : IRequest<Result<TransactionDto>>;

public record TransactionDto(
    Guid Id, Guid UserId, decimal Amount, string Type,
    string? Description, string? Note, Guid? CategoryId,
    CategoryDto? Category, DateTime OccurredAt, DateTime CreatedAt
);

public record CategoryDto(Guid Id, string Name, string? Icon, string? Color);

public class CreateTransactionCommandHandler
    : IRequestHandler<CreateTransactionCommand, Result<TransactionDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICacheService _cache;

    public CreateTransactionCommandHandler(IAppDbContext db, ICacheService cache)
        => (_db, _cache) = (db, cache);

    public async Task<Result<TransactionDto>> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        // Idempotency check
        if (request.IdempotencyKey is not null)
        {
            var existing = await _db.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.IdempotencyKey == request.IdempotencyKey, ct);

            if (existing is not null) return Result<TransactionDto>.Success(MapToDto(existing));
        }

        var tx = Transaction.Create(
            request.UserId, request.Amount, request.Type,
            request.Description, request.Note, request.CategoryId,
            request.OccurredAt, request.IdempotencyKey
        );

        _db.Transactions.Add(tx);
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await Task.WhenAll(
            _cache.InvalidateAsync(CacheKeys.Dashboard(request.UserId), ct),
            _cache.InvalidateAsync(CacheKeys.Transactions(request.UserId), ct)
        );

        return Result<TransactionDto>.Success(MapToDto(tx));
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.UserId, t.Amount, t.Type.ToString(),
        t.Description, t.Note, t.CategoryId,
        t.Category is null ? null : new CategoryDto(t.Category.Id, t.Category.Name, t.Category.Icon, t.Category.Color),
        t.OccurredAt, t.CreatedAt
    );
}
