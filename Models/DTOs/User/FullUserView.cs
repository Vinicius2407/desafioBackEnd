using Models.DTOs.Bet;
using Models.DTOs.Transaction;
using Models.DTOs.Wallet;
using System.Text.Json.Serialization;

namespace Models.DTOs.User;
public class FullUserView : UserViewModel
{
    public string Document { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public WalletViewModel Wallet { get; set; } = null!;
    public List<TransactionViewModel>? Transactions { get; set; } = null;
    public List<BetViewModel> Bets { get; set; } = new List<BetViewModel>();
    public int LoseStreakCounter { get; set; }
    [JsonIgnore]
    public decimal LostBetAmountPercentage { get; set; }
    public bool IsOnLosingStreak()
    {
        var betsOrdenada = Bets.Where(x => x.Status == Helpers.Enumerators.BetStatus.LOST && LoseStreakCounter >= 5).OrderByDescending(bet => bet.CreatedAt).Take(5).ToList();
        LostBetAmountPercentage = betsOrdenada.Sum(x => x.Amount) * 0.1M;

        return LoseStreakCounter >= 5;
    }
}
