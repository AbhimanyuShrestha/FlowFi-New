using FlowFi.Domain.Common;
using FlowFi.Domain.Enums;

namespace FlowFi.Domain.Entities;

public class Transaction : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Description { get; private set; }
    public string? Note { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public TransactionSource Source { get; private set; } = TransactionSource.Manual;

    public User User { get; private set; } = default!;
    public Category? Category { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        Guid userId, decimal amount, TransactionType type,
        string? description, string? note, Guid? categoryId,
        DateTime? occurredAt, string? idempotencyKey) =>
        new()
        {
            UserId = userId, Amount = amount, Type = type,
            Description = description, Note = note, CategoryId = categoryId,
            OccurredAt = occurredAt ?? DateTime.UtcNow,
            IdempotencyKey = idempotencyKey,
        };

    public void Update(decimal? amount, TransactionType? type, string? description,
                       string? note, Guid? categoryId, DateTime? occurredAt)
    {
        if (amount.HasValue) Amount = amount.Value;
        if (type.HasValue) Type = type.Value;
        if (description is not null) Description = description;
        if (note is not null) Note = note;
        if (categoryId.HasValue) CategoryId = categoryId.Value;
        if (occurredAt.HasValue) OccurredAt = occurredAt.Value;
        SetUpdated();
    }
}
