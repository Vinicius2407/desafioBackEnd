namespace Models.Helpers;
public class Enumerators
{
    public enum TransactionType
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
