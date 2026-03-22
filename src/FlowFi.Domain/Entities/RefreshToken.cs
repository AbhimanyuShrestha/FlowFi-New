using FlowFi.Domain.Common;

namespace FlowFi.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public bool Revoked { get; private set; }

    public User User { get; private set; } = default!;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAt) =>
        new() { UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt, Revoked = false };

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsActive => !Revoked && !IsExpired;

    public void Revoke() => Revoked = true;
}
