using Models.DTOs.Bet;
using Models.DTOs.Transaction;
using Models.DTOs.Wallet;

namespace Models.DTOs.User;
public class FullUserView : UserViewModel
{
    public string Document { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public WalletViewModel Wallet { get; set; } = null!;
    public List<TransactionViewModel>? Transactions { get; set; } = null;
    public List<BetViewModel> Bets { get; set; } = new List<BetViewModel>();

    public bool IsOnLosingStreak
    {
        get
        {
            var betsOrdenada = Bets.OrderByDescending(bet => bet.CreatedAt).Take(5).ToList();

            if (betsOrdenada.Count < 5)
                return false;

            return betsOrdenada.All(x => x.Status == Helpers.Enumerators.BetStatus.LOST);
        }
    }
}
