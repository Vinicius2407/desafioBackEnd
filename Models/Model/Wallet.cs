using Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Model;
public class Wallet : BaseEntity, ISoftDelete
{
    public long UserId { get; set; }
    public long CurrencyId { get; set; }
    public decimal BalanceAvailable { get; set; }
    public decimal BalanceBlocked { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public User User { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public uint xmin { get; private set; }
}
