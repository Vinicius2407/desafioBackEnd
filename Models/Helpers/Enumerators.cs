using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Helpers;
public class Enumerators
{
    public enum  TransactionType
    {
        DEPOSIT,
        WITHDRAW,
        BET,
        WIN,
        LOSS,
        REFUND
    }

    public enum BetStatus
    {
        PENDING,
        WON,
        LOST,
        CANCELLED
    }
}
