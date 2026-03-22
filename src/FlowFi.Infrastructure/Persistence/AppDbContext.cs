using FlowFi.Application.Common.Interfaces;
using FlowFi.Domain.Entities;
using FlowFi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlowFi.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User>           Users           => Set<User>();
    public DbSet<RefreshToken>   RefreshTokens   => Set<RefreshToken>();
    public DbSet<Category>       Categories      => Set<Category>();
    public DbSet<Transaction>    Transactions    => Set<Transaction>();
    public DbSet<Prediction>     Predictions     => Set<Prediction>();
    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<UserPlan>().HaveConversion<string>();
        builder.Properties<RiskLevel>().HaveConversion<string>();
        builder.Properties<TransactionType>().HaveConversion<string>();
        builder.Properties<TransactionSource>().HaveConversion<string>();
    }
}
