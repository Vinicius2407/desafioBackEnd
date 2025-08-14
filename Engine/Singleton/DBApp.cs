using Microsoft.EntityFrameworkCore;
using Models.Model;

namespace Engine.Singleton;
public class DBApp : DbContext
{
    public DBApp(DbContextOptions<DBApp> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DBApp).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}