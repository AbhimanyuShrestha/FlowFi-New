using FlowFi.Domain.Common;
using FlowFi.Domain.Enums;

namespace FlowFi.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string? FullName { get; private set; }
    public string Currency { get; private set; } = "USD";
    public string Timezone { get; private set; } = "UTC";
    public UserPlan Plan { get; private set; } = UserPlan.Free;

    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<Recommendation> Recommendations { get; private set; } = new List<Recommendation>();

    private User() { } // EF Core

    public static User Create(string email, string passwordHash, string? fullName, string currency) =>
        new() { Email = email.ToLowerInvariant(), PasswordHash = passwordHash, FullName = fullName, Currency = currency };

    public void UpdateProfile(string? fullName, string? currency, string? timezone)
    {
        if (fullName is not null) FullName = fullName;
        if (currency is not null) Currency = currency;
        if (timezone is not null) Timezone = timezone;
        SetUpdated();
    }

    public void UpgradePlan(UserPlan plan) { Plan = plan; SetUpdated(); }
}
