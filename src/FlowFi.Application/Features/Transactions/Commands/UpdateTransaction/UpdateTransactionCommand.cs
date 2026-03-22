using FlowFi.Application.Common;
using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using FlowFi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Transactions.Commands.UpdateTransaction;

public record UpdateTransactionCommand(
    Guid UserId,
    Guid TransactionId,
    decimal? Amount,
    TransactionType? Type,
    string? Description,
    string? Note,
    Guid? CategoryId,
    DateTime? OccurredAt
) : IRequest<Result<TransactionDto>>;

public class UpdateTransactionCommandHandler
    : IRequestHandler<UpdateTransactionCommand, Result<TransactionDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICacheService _cache;

    public UpdateTransactionCommandHandler(IAppDbContext db, ICacheService cache)
        => (_db, _cache) = (db, cache);

    public async Task<Result<TransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken ct)
    {
        var tx = await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == request.UserId, ct);

        if (tx is null) return Result<TransactionDto>.NotFound("Transaction");

        tx.Update(
            request.Amount, request.Type, request.Description,
            request.Note, request.CategoryId, request.OccurredAt
        );

        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await Task.WhenAll(
            _cache.InvalidateAsync(CacheKeys.Dashboard(request.UserId), ct),
            _cache.InvalidateAsync(CacheKeys.Transactions(request.UserId), ct)
        );

        // Re-fetch to get updated Category if it changed
        if (request.CategoryId.HasValue)
        {
             tx = await _db.Transactions
                .Include(t => t.Category)
                .FirstAsync(t => t.Id == tx.Id, ct);
        }

        return Result<TransactionDto>.Success(MapToDto(tx));
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.UserId, t.Amount, t.Type.ToString(),
        t.Description, t.Note, t.CategoryId,
        t.Category is null ? null : new CategoryDto(t.Category.Id, t.Category.Name, t.Category.Icon, t.Category.Color),
        t.OccurredAt, t.CreatedAt
    );
}
