using FlowFi.Application.Common;
using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Transactions.Commands.DeleteTransaction;

public record DeleteTransactionCommand(Guid UserId, Guid TransactionId) : IRequest<Result>;

public class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Result>
{
    private readonly IAppDbContext _db;
    private readonly ICacheService _cache;

    public DeleteTransactionCommandHandler(IAppDbContext db, ICacheService cache)
        => (_db, _cache) = (db, cache);

    public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken ct)
    {
        var tx = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == request.UserId, ct);

        if (tx is null) return Result.NotFound("Transaction");

        _db.Transactions.Remove(tx);
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await Task.WhenAll(
            _cache.InvalidateAsync(CacheKeys.Dashboard(request.UserId), ct),
            _cache.InvalidateAsync(CacheKeys.Transactions(request.UserId), ct)
        );

        return Result.Success();
    }
}
