using FlowFi.Domain.Entities;

namespace FlowFi.Application.Common.Interfaces;

public interface IAiService
{
    Task<Prediction> ComputePredictionAsync(Guid userId, CancellationToken ct = default);
    Task<RiskAssessment> ComputeRiskScoreAsync(Guid userId, CancellationToken ct = default);
    Task GenerateRecommendationsAsync(Guid userId, CancellationToken ct = default);
}
