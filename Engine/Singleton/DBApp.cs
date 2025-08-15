using Microsoft.EntityFrameworkCore;
using Models.Model;
using System.Linq.Expressions;

namespace Engine.Singleton;
public class DBApp : DbContext
{
    public DBApp(DbContextOptions<DBApp> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<Bet> Bets { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBApp).Assembly);

        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.FindProperty("IsDeleted") != null)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, "IsDeleted"),
                        Expression.Constant(false)
                    ),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted && e.Entity.GetType().GetProperty("IsDeleted") != null))
        {
            entry.State = EntityState.Modified;
            entry.CurrentValues["IsDeleted"] = true;
            entry.CurrentValues["DeletedAt"] = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted && e.Entity.GetType().GetProperty("IsDeleted") != null))
        {
            entry.State = EntityState.Modified;
            entry.CurrentValues["IsDeleted"] = true;
            entry.CurrentValues["DeletedAt"] = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}