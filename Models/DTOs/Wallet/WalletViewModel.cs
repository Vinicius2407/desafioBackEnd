using Models.DTOs.Currency;
using Models.DTOs.Transaction;
using Models.DTOs.User;
using Models.Model;

namespace Models.DTOs.Wallet;
public class WalletViewModel : BaseEntity
{
    public long UserId { get; set; }
    public decimal BalanceAvailable { get; set; }
    public decimal BalanceBlocked { get; set; }
    public CurrencyViewModel Currency { get; set; } = null!;
    public UserViewModel? User { get; set; } = null;
    public List<TransactionViewModel>? Transactions { get; set; } = null;
}
