using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Model;

namespace Engine.ModelMap;
public class TransactionMap : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.WalletId).IsRequired();
        builder.Property(x => x.BetId).IsRequired(false);
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Type).HasConversion<string>().IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasOne(x => x.Wallet)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Bet)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.BetId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
