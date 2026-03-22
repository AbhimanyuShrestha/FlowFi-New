using FlowFi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowFi.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Category> Categories { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Prediction> Predictions { get; }
    DbSet<RiskAssessment> RiskAssessments { get; }
    DbSet<Recommendation> Recommendations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
