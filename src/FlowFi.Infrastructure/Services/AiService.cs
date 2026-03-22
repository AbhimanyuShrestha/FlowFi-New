using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Entities;
using FlowFi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IAppDbContext _db;
    private const string ModelVersion = "rule-v1";

    public AiService(IAppDbContext db) => _db = db;

    public async Task<Prediction> ComputePredictionAsync(Guid userId, CancellationToken ct = default)
    {
        var now           = DateTime.UtcNow;
        var threeMonthsAgo = now.AddMonths(-3);

        var transactions = await _db.Transactions
            .Where(t => t.UserId == userId && t.OccurredAt >= threeMonthsAgo && t.Type == TransactionType.Expense)
            .Select(t => new { t.Amount, t.OccurredAt })
            .ToListAsync(ct);

        var currentMonthKey = $"{now.Year}-{now.Month:D2}";

        var monthly = transactions
            .GroupBy(t => $"{t.OccurredAt.Year}-{t.OccurredAt.Month:D2}")
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var historicalAvg = monthly
            .Where(kv => kv.Key != currentMonthKey)
            .Select(kv => kv.Value)
            .DefaultIfEmpty(0m)
            .Average();

        var currentSpend  = monthly.GetValueOrDefault(currentMonthKey, 0m);
        var daysElapsed   = now.Day;
        var daysInMonth   = DateTime.DaysInMonth(now.Year, now.Month);
        var projectedPace = daysElapsed > 0 ? (currentSpend / daysElapsed) * daysInMonth : 0m;
        var predicted     = historicalAvg * 0.7m + projectedPace * 0.3m;

        var confidence    = 0.70;
        var totalDays     = (now - threeMonthsAgo).TotalDays;
        if (totalDays >= 60) confidence += 0.10;

        var variance = historicalAvg > 0 ? (double)(Math.Abs(projectedPace - historicalAvg) / historicalAvg) : 1;
        if (variance < 0.20) confidence += 0.10;

        var currentMonthCount = transactions.Count(t => t.OccurredAt.Year == now.Year && t.OccurredAt.Month == now.Month);
        if (currentMonthCount < 10) confidence -= 0.20;

        confidence = Math.Max(0, Math.Min(1, confidence));

        return Prediction.Create(
            userId,
            new DateOnly(now.Year, now.Month, 1),
            new DateOnly(now.Year, now.Month, daysInMonth),
            Math.Round(predicted, 2),
            Math.Round(confidence, 3),
            ModelVersion
        );
    }

    public async Task<RiskAssessment> ComputeRiskScoreAsync(Guid userId, CancellationToken ct = default)
    {
        var now            = DateTime.UtcNow;
        var threeMonthsAgo = now.AddMonths(-3);
        var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var txs = await _db.Transactions
            .Where(t => t.UserId == userId && t.OccurredAt >= threeMonthsAgo)
            .Select(t => new { t.Amount, t.Type, t.CategoryId, t.OccurredAt })
            .ToListAsync(ct);

        var score   = 100;
        var factors = new List<RiskFactor>();

        var currentExpenses     = txs.Where(t => t.Type == TransactionType.Expense && t.OccurredAt >= currentMonthStart).Sum(t => t.Amount);
        var historicalExpenses  = txs.Where(t => t.Type == TransactionType.Expense && t.OccurredAt < currentMonthStart).ToList();
        var uniqueMonths        = historicalExpenses.Select(t => $"{t.OccurredAt.Year}-{t.OccurredAt.Month}").Distinct().Count();
        var avgMonthly          = uniqueMonths > 0 ? historicalExpenses.Sum(t => t.Amount) / uniqueMonths : 0m;
        var projected           = now.Day > 0 ? (currentExpenses / now.Day) * DateTime.DaysInMonth(now.Year, now.Month) : 0m;

        if (avgMonthly > 0 && projected > avgMonthly * 1.1m)
        {
            score -= 20;
            factors.Add(new RiskFactor("budget_pace",
                $"On track to spend {Math.Round((projected / avgMonthly - 1) * 100)}% over monthly average",
                RiskLevel.Medium));
        }

        var totalIncome  = txs.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = txs.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        if (totalIncome > 0 && totalExpense / totalIncome > 0.8m)
        {
            score -= 20;
            factors.Add(new RiskFactor("income_ratio",
                $"Expenses are {Math.Round(totalExpense / totalIncome * 100)}% of income",
                RiskLevel.High));
        }

        var lastTx = txs.MaxBy(t => t.OccurredAt);
        if (lastTx is not null && (now - lastTx.OccurredAt).TotalDays > 14)
        {
            score -= 10;
            factors.Add(new RiskFactor("data_freshness",
                $"No transactions logged in {Math.Round((now - lastTx.OccurredAt).TotalDays)} days",
                RiskLevel.Low));
        }

        if (totalIncome > 0)
        {
            var savingsRate = (totalIncome - totalExpense) / totalIncome;
            if (savingsRate < 0.1m)
            {
                score -= 10;
                factors.Add(new RiskFactor("savings_rate",
                    $"Savings rate is {Math.Round(savingsRate * 100)}%, below recommended 10%",
                    RiskLevel.Medium));
            }
        }

        score = Math.Max(0, Math.Min(100, score));
        var riskLevel = score switch
        {
            >= 80 => RiskLevel.Low,
            >= 60 => RiskLevel.Medium,
            >= 40 => RiskLevel.High,
            _     => RiskLevel.Critical,
        };

        return RiskAssessment.Create(userId, score, riskLevel, factors);
    }

    public async Task GenerateRecommendationsAsync(Guid userId, CancellationToken ct = default)
    {
        var sevenDaysAgo   = DateTime.UtcNow.AddDays(-7);
        var recentTypes    = await _db.Recommendations
            .Where(r => r.UserId == userId && r.CreatedAt >= sevenDaysAgo)
            .Select(r => r.Type)
            .ToListAsync(ct);

        var risk       = await ComputeRiskScoreAsync(userId, ct);
        var expiresAt  = DateTime.UtcNow.AddDays(7);

        var toAdd = new List<Recommendation>();

        var budgetFactor = risk.Factors.FirstOrDefault(f => f.Id == "budget_pace");
        if (budgetFactor is not null && !recentTypes.Contains("reduce_category"))
            toAdd.Add(Recommendation.Create(userId, "reduce_category",
                "Reduce spending this month", budgetFactor.Label, 1, expiresAt));

        var savingsFactor = risk.Factors.FirstOrDefault(f => f.Id == "savings_rate");
        if (savingsFactor is not null && !recentTypes.Contains("increase_savings"))
            toAdd.Add(Recommendation.Create(userId, "increase_savings",
                "Boost your savings rate", savingsFactor.Label, 2, expiresAt));

        var freshnessFactor = risk.Factors.FirstOrDefault(f => f.Id == "log_reminder");
        if (freshnessFactor is not null && !recentTypes.Contains("log_reminder"))
            toAdd.Add(Recommendation.Create(userId, "log_reminder",
                "Log your recent transactions", freshnessFactor.Label, 5, expiresAt));

        if (risk.RiskLevel == RiskLevel.Critical && !recentTypes.Contains("critical_alert"))
            toAdd.Add(Recommendation.Create(userId, "critical_alert",
                "Your financial health needs attention",
                "Your health score is critical. Review your spending immediately.", 1, expiresAt));

        if (toAdd.Count > 0)
        {
            _db.Recommendations.AddRange(toAdd);
            await _db.SaveChangesAsync(ct);
        }
    }
}
