using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Investment.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Institution).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(500);
        builder.Property(x => x.AnnualRatePercent).HasPrecision(5, 2);

        builder.OwnsOne(x => x.Principal, m =>
        {
            m.Property(mo => mo.Amount).HasColumnName("Principal").HasPrecision(18, 4);
            m.OwnsOne(mo => mo.Currency, c =>
                c.Property(cc => cc.Value).HasColumnName("Currency").HasMaxLength(3));
        });
        builder.OwnsOne(x => x.MaturityValue, m =>
        {
            m.Property(mo => mo.Amount).HasColumnName("MaturityValue").HasPrecision(18, 4);
            m.OwnsOne(mo => mo.Currency, c =>
                c.Property(cc => cc.Value).HasColumnName("MaturityCurrency").HasMaxLength(3));
        });

        builder.Ignore(x => x.DomainEvents);
        builder.ToTable("Investments");
    }
}
