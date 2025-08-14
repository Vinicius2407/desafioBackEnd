using Models.Helpers;
using Models.Interfaces;

namespace Models.Model;
public class Bet : BaseEntity, ISoftDelete
{
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public decimal? PrizeAmount { get; set; } = null;
    public Enumerators.BetStatus Status { get; set; } = Enumerators.BetStatus.PENDING;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }

    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    public User User { get; set; } = null!;
}