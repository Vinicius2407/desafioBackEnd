using Models.Model;

namespace Models.DTOs.User;
public class UserViewModel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
