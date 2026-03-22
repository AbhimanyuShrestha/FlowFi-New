using FlowFi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowFi.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Amount).HasPrecision(12, 2).IsRequired();
        builder.Property(t => t.Type).HasMaxLength(10).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(255);
        builder.Property(t => t.IdempotencyKey).HasMaxLength(100);
        builder.HasIndex(t => t.IdempotencyKey).IsUnique().HasFilter("idempotency_key IS NOT NULL");
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.OccurredAt);
        builder.HasOne(t => t.User).WithMany(u => u.Transactions).HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Category).WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.SetNull);
    }
}
