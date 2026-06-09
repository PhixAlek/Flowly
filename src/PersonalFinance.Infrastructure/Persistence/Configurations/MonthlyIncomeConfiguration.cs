using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Income.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class MonthlyIncomeConfiguration : IEntityTypeConfiguration<MonthlyIncome>
{
    public void Configure(EntityTypeBuilder<MonthlyIncome> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();

        // DatePeriod owned value object
        builder.OwnsOne(x => x.Period, p =>
        {
            p.Property(d => d.Year).HasColumnName("PeriodYear").IsRequired();
            p.Property(d => d.Month).HasColumnName("PeriodMonth").IsRequired();
            p.HasIndex(d => new { d.Year, d.Month });
        });

        // CurrencyCode owned value object
        builder.OwnsOne(x => x.HomeCurrency, c =>
            c.Property(cc => cc.Value).HasColumnName("HomeCurrency").HasMaxLength(3).IsRequired());

        // IncomeEntry child entities (one-to-many)
        builder.HasMany<IncomeEntry>("_entries")
            .WithOne()
            .HasForeignKey("MonthlyIncomeId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_entries").HasField("_entries").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => new { x.UserId });
        builder.ToTable("MonthlyIncomes");
    }
}
