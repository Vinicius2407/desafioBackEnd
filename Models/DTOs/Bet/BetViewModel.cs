using Models.DTOs.Transaction;
using Models.DTOs.User;
using Models.Helpers;
using Models.Model;
using System.Text.Json.Serialization;

namespace Models.DTOs.Bet;
public class BetViewModel : BaseEntity
{
    [JsonIgnore]
    public long UserId { get; set; }
    public decimal Amount { get; set; }
    public decimal? PrizeAmount { get; set; } = null;
    public Enumerators.BetStatus Status { get; set; } = Enumerators.BetStatus.PENDING;
    public string StatusDescription { get => Status.ToString(); }
    public List<TransactionViewModel>? Transactions { get; set; } = null;
    [JsonIgnore]
    public UserViewModel? User { get; set; } = null;
    public new long? Id { get; set; } = null;
}
