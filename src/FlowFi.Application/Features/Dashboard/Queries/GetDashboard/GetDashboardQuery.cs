using FlowFi.Application.Common;
using FlowFi.Application.Common.Interfaces;
using FlowFi.Application.Features.Transactions.Commands.CreateTransaction;
using FlowFi.Domain.Common;
using FlowFi.Domain.Entities;
using FlowFi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Features.Dashboard.Queries.GetDashboard;

public record GetDashboardQuery(Guid UserId) : IRequest<Result<DashboardDto>>;

public record DashboardDto(
    PeriodDto Period,
    SummaryDto Summary,
    int HealthScore,
    RiskDto Risk,
    PredictionDto Prediction,
    IReadOnlyList<RecommendationDto> Recommendations,
    IReadOnlyList<SpendingByCategoryDto> SpendingByCategory,
    IReadOnlyList<TransactionDto> RecentTransactions
);

public record PeriodDto(DateTime Start, DateTime End);
public record SummaryDto(decimal TotalIncome, decimal TotalExpenses, decimal Net, double SavingsRate);
public record RiskDto(string Level, IReadOnlyList<RiskFactorDto> Factors);
public record RiskFactorDto(string Id, string Label, string Severity);
public record PredictionDto(decimal ProjectedBalance, double Confidence, string ModelVersion);
public record RecommendationDto(Guid Id, string Type, string Title, string Body, int Priority);
public record SpendingByCategoryDto(string Category, decimal Amount, double Percentage);

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICacheService _cache;
    private readonly IAiService _ai;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    public GetDashboardQueryHandler(IAppDbContext db, ICacheService cache, IAiService ai)
        => (_db, _cache, _ai) = (db, cache, ai);

    public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var cacheKey = CacheKeys.Dashboard(request.UserId);
        var cached = await _cache.GetAsync<DashboardDto>(cacheKey, ct);
        if (cached is not null) return Result<DashboardDto>.Success(cached);

        var now         = DateTime.UtcNow;
        var monthStart  = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd    = monthStart.AddMonths(1).AddTicks(-1);

        var txTask     = _db.Transactions.Include(t => t.Category)
                            .Where(t => t.UserId == request.UserId && t.OccurredAt >= monthStart && t.OccurredAt <= monthEnd)
                            .OrderByDescending(t => t.OccurredAt).ToListAsync(ct);

        var recsTask   = _db.Recommendations
                            .Where(r => r.UserId == request.UserId && !r.Dismissed)
                            .OrderBy(r => r.Priority).Take(3).ToListAsync(ct);

        var predTask   = _ai.ComputePredictionAsync(request.UserId, ct);
        var riskTask   = _ai.ComputeRiskScoreAsync(request.UserId, ct);

        await Task.WhenAll(txTask, recsTask, predTask, riskTask);

        var txs         = await txTask;
        var recs        = await recsTask;
        var prediction  = await predTask;
        var risk        = await riskTask;

        var income      = txs.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expenses    = txs.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var net         = income - expenses;
        var savingsRate = income > 0 ? (double)(net / income) : 0;

        var byCategory  = txs
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category?.Name ?? "Other")
            .Select(g => new SpendingByCategoryDto(
                g.Key, g.Sum(t => t.Amount),
                expenses > 0 ? (double)(g.Sum(t => t.Amount) / expenses) : 0))
            .OrderByDescending(c => c.Amount)
            .Take(6).ToList();

        var dashboard = new DashboardDto(
            new PeriodDto(monthStart, monthEnd),
            new SummaryDto(income, expenses, net, savingsRate),
            risk.HealthScore,
            new RiskDto(risk.RiskLevel.ToString(), risk.Factors.Select(f =>
                new RiskFactorDto(f.Id, f.Label, f.Severity.ToString())).ToList()),
            new PredictionDto(
                net - prediction.PredictedExpenses + expenses,
                prediction.ConfidenceScore, prediction.ModelVersion),
            recs.Select(r => new RecommendationDto(r.Id, r.Type, r.Title, r.Body, r.Priority)).ToList(),
            byCategory,
            txs.Take(10).Select(t => new TransactionDto(
                t.Id, t.UserId, t.Amount, t.Type.ToString(), t.Description, t.Note, t.CategoryId,
                t.Category is null ? null : new CategoryDto(t.Category.Id, t.Category.Name, t.Category.Icon, t.Category.Color),
                t.OccurredAt, t.CreatedAt)).ToList()
        );

        await _cache.SetAsync(cacheKey, dashboard, _cacheTtl, ct);
        return Result<DashboardDto>.Success(dashboard);
    }
}
