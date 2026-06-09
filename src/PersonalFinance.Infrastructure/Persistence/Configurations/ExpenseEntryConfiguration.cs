using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Expense.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public sealed class ExpenseEntryConfiguration : IEntityTypeConfiguration<ExpenseEntry>
{
    public void Configure(EntityTypeBuilder<ExpenseEntry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Description).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.Nature).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Merchant).HasMaxLength(200);
        builder.Property(x => x.ReceiptImageUrl).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.OwnsOne(x => x.Amount, m =>
        {
            m.Property(mo => mo.Amount).HasColumnName("Amount").HasPrecision(18, 4);
            m.OwnsOne(mo => mo.Currency, c =>
                c.Property(cc => cc.Value).HasColumnName("Currency").HasMaxLength(3));
        });

        builder.Ignore(x => x.DomainEvents);
        builder.ToTable("ExpenseEntries");
    }
}
