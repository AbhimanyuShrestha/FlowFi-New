using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Transactions.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid UserId, Guid TransactionId) : IRequest<Result<TransactionDto>>;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionDto>>
{
    private readonly IAppDbContext _db;

    public GetTransactionByIdQueryHandler(IAppDbContext db) => _db = db;

    public async Task<Result<TransactionDto>> Handle(GetTransactionByIdQuery request, CancellationToken ct)
    {
        var transaction = await _db.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == request.UserId, ct);

        if (transaction is null)
            return Result<TransactionDto>.NotFound("Transaction");

        return Result<TransactionDto>.Success(MapToDto(transaction));
    }

    private static TransactionDto MapToDto(Transaction t) => new(
        t.Id, t.UserId, t.Amount, t.Type.ToString(),
        t.Description, t.Note, t.CategoryId,
        t.Category is null ? null : new CategoryDto(t.Category.Id, t.Category.Name, t.Category.Icon, t.Category.Color),
        t.OccurredAt, t.CreatedAt
    );
}
