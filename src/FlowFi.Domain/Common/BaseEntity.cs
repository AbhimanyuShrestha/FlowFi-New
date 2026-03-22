namespace FlowFi.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

    public void SetUpdated() => UpdatedAt = DateTime.UtcNow;
}
