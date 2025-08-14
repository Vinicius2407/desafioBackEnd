using Models.DTOs.Transaction;
using Models.DTOs.User;
using Models.Helpers;
using Models.Model;

namespace Models.DTOs.Bet;
public class BetViewModel : BaseEntity
{
    public decimal Amout { get; set; }
    public decimal? PrizeAmount { get; set; } = null;
    public Enumerators.BetStatus Status { get; set; } = Enumerators.BetStatus.PENDING;
    public List<TransactionViewModel>? Transactions { get; set; } = null;
    public UserViewModel? User { get; set; } = null;
}
