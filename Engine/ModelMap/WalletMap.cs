using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Model;

namespace Engine.ModelMap;
public class WalletMap : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.BalanceAvailable);
        builder.Property(x => x.BalanceBlocked);
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.User)
            .WithOne(x => x.Wallet)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(w => w.xmin)
            .HasColumnName("xmin")
            .IsRowVersion()
            .ValueGeneratedOnAddOrUpdate();
    }
}
