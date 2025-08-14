using Models.DTOs.Wallet;

namespace Models.DTOs.User;
public class FullUserView : UserViewModel
{
    public string Document { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public WalletViewModel Wallet { get; set; } = null!;
}
