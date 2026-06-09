using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Expense.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class MonthlyBudgetConfiguration : IEntityTypeConfiguration<MonthlyBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyBudget> builder)
    {
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Period, p =>
        {
            p.Property(d => d.Year).HasColumnName("PeriodYear");
            p.Property(d => d.Month).HasColumnName("PeriodMonth");
        });

        builder.OwnsOne(x => x.HomeCurrency, c =>
            c.Property(cc => cc.Value).HasColumnName("HomeCurrency").HasMaxLength(3));

        builder.HasMany<ExpenseEntry>("_entries")
            .WithOne()
            .HasForeignKey("MonthlyBudgetId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_entries").HasField("_entries").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
        builder.ToTable("MonthlyBudgets");
    }
}
