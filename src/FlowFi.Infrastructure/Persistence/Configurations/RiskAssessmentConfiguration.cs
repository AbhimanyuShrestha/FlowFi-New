using FlowFi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace FlowFi.Infrastructure.Persistence.Configurations;

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.UserId, r.AssessedAt });

        builder.Property(r => r.Factors)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<RiskFactor>>(v, (JsonSerializerOptions?)null) ?? new()
            );
    }
}
