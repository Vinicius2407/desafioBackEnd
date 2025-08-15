using Models.Interfaces;

namespace Models.Model;
public class User : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int LoseStreakCounter { get; set; } = 0;

    public Wallet Wallet { get; set; } = null!;
    public List<Bet> Bets { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
}
