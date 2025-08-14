using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Model;

namespace Engine.ModelMap;
public class BetMap : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("Bets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.PrizeAmount).IsRequired(false);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired().HasDefaultValue(false);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Bets)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
