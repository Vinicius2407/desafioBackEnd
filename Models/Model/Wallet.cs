using Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public User User { get; set; } = new();
    public Currency Currency { get; set; } = new();
    public List<Transaction> Transactions { get; set; } = new List<Transaction>();
}
