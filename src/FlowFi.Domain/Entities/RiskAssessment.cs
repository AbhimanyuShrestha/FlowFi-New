using FlowFi.Domain.Common;
using FlowFi.Domain.Enums;

namespace FlowFi.Domain.Entities;

public record RiskFactor(string Id, string Label, RiskLevel Severity);

public class RiskAssessment : BaseEntity
{
    public Guid UserId { get; private set; }
    public int HealthScore { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public List<RiskFactor> Factors { get; private set; } = new();
    public DateTime AssessedAt { get; private set; } = DateTime.UtcNow;

    private RiskAssessment() { }

    public static RiskAssessment Create(Guid userId, int healthScore,
        RiskLevel riskLevel, List<RiskFactor> factors) =>
        new() { UserId = userId, HealthScore = healthScore, RiskLevel = riskLevel, Factors = factors };
}
