using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Savings.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
{
    public void Configure(EntityTypeBuilder<SavingsGoal> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GoalType).HasConversion<string>().HasMaxLength(50);

        builder.OwnsOne(x => x.TargetAmount, m =>
        {
            m.Property(mo => mo.Amount).HasColumnName("TargetAmount").HasPrecision(18, 4);
            m.OwnsOne(mo => mo.Currency, c =>
                c.Property(cc => cc.Value).HasColumnName("Currency").HasMaxLength(3));
        });
        builder.OwnsOne(x => x.CurrentAmount, m =>
        {
            m.Property(mo => mo.Amount).HasColumnName("CurrentAmount").HasPrecision(18, 4);
            m.OwnsOne(mo => mo.Currency, c =>
                c.Property(cc => cc.Value).HasColumnName("CurrentCurrency").HasMaxLength(3));
        });

        builder.Ignore(x => x.DomainEvents);
        builder.ToTable("SavingsGoals");
    }
}
