using FlowFi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowFi.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(100);
        builder.Property(u => u.Currency).HasMaxLength(3).IsRequired();
        builder.Property(u => u.Timezone).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Plan).HasMaxLength(20).IsRequired();
    }
}
