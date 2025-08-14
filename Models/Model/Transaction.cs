using Models.Interfaces;
using Models.Helpers;

namespace Models.Model;
public class Transaction : BaseEntity, ISoftDelete
{
    public long WalletId { get; set; }
    public long? BetId { get; set; }
    public decimal Amount { get; set; }
    public Enumerators.TransactionType Type { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public Bet? Bet { get; set; } = null;
    public Wallet Wallet { get; set; } = new();
}