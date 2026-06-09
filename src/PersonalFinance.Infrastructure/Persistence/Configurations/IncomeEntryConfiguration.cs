using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Income.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class IncomeEntryConfiguration : IEntityTypeConfiguration<IncomeEntry>
{
    public void Configure(EntityTypeBuilder<IncomeEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Nature).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.ContractType).HasConversion<string>().HasMaxLength(50);

        // TaxInfo owned value object
        builder.OwnsOne(x => x.TaxInfo, t =>
        {
            t.Property(ti => ti.GrossAmount).HasColumnName("GrossAmount").HasPrecision(18, 4);
            t.Property(ti => ti.DeductionRatePercent).HasColumnName("DeductionRatePercent").HasPrecision(5, 2);
        });

        // OriginalCurrency owned value object
        builder.OwnsOne(x => x.OriginalCurrency, c =>
            c.Property(cc => cc.Value).HasColumnName("OriginalCurrency").HasMaxLength(3).IsRequired());

        // ConvertedAmount owned value object (optional)
        builder.OwnsOne(x => x.ConvertedAmount, m =>
        {
            m.Property(mo => mo.Amount).HasColumnName("ConvertedAmount").HasPrecision(18, 4);
            m.OwnsOne(mo => mo.Currency, c =>
                c.Property(cc => cc.Value).HasColumnName("ConvertedCurrency").HasMaxLength(3));
        });

        builder.Property(x => x.ExchangeRateUsed).HasPrecision(18, 8);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Ignore(x => x.DomainEvents);
        builder.ToTable("IncomeEntries");
    }
}
