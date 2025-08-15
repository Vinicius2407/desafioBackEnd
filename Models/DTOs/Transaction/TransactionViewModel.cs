using Models.DTOs.Bet;
using Models.Helpers;
using Models.Model;

namespace Models.DTOs.Transaction;
public class TransactionViewModel : BaseEntity
{
    public decimal Amount { get; set; }
    public Enumerators.TransactionType Type { get; set; }
    public string TypeDescription { get => Type.ToString(); }
    public string? Description { get; set; }
    public long? BetId { get; set; }
    public long? WalletId { get; set; }
    public BetViewModel? Bet { get; set; } = null;
}
