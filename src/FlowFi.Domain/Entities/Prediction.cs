using FlowFi.Domain.Common;

namespace FlowFi.Domain.Entities;

public class Prediction : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public decimal PredictedExpenses { get; private set; }
    public decimal? PredictedIncome { get; private set; }
    public double ConfidenceScore { get; private set; }
    public string ModelVersion { get; private set; } = "rule-v1";
    public DateTime GeneratedAt { get; private set; } = DateTime.UtcNow;

    private Prediction() { }

    public static Prediction Create(Guid userId, DateOnly start, DateOnly end,
        decimal predictedExpenses, double confidence, string modelVersion = "rule-v1") =>
        new()
        {
            UserId = userId, PeriodStart = start, PeriodEnd = end,
            PredictedExpenses = predictedExpenses, ConfidenceScore = confidence,
            ModelVersion = modelVersion,
        };
}
