using Models.DTOs.Bet;
using Models.Helpers;
using Models.Model;

namespace Models.DTOs.Transaction;
public class TransactionViewModel : BaseEntity
{
    public decimal Amount { get; set; }
    public Enumerators.TransactionType Type { get; set; }
    public string? Description { get; set; }

    public BetViewModel? Bet { get; set; } = null;
}
