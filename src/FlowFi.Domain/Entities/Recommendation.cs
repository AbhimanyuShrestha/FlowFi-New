using FlowFi.Domain.Common;

namespace FlowFi.Domain.Entities;

public class Recommendation : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Type { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public string? ActionLabel { get; private set; }
    public int Priority { get; private set; } = 5;
    public bool Dismissed { get; private set; }
    public bool ActedOn { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public User User { get; private set; } = default!;

    private Recommendation() { }

    public static Recommendation Create(Guid userId, string type, string title,
        string body, int priority, DateTime? expiresAt, string? actionLabel = null) =>
        new()
        {
            UserId = userId, Type = type, Title = title, Body = body,
            Priority = priority, ExpiresAt = expiresAt, ActionLabel = actionLabel,
        };

    public void Dismiss() => Dismissed = true;
    public void MarkActedOn() => ActedOn = true;
}
