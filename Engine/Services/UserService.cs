using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.User;
using Models.Model;

namespace Engine.Services;
public class UserService : IService
{
    private readonly DBApp _context;
    public UserService(DBApp context)
    {
        _context = context;
    }

    public async Task<UserViewModel> SignUp(CreateUserDto createUserDto)
    {
        var passwordHash = Engine.Helpers.PasswordHelper.HashPassword(createUserDto.Password);

        var user = new User
        {
            Name = createUserDto.Name,
            Password = passwordHash,
            Email = createUserDto.Email,
            Document = createUserDto.Document,
            PhoneNumber = createUserDto.PhoneNumber
        };

        var userView = await SaveChanges(user);

        if (userView.HasErrors)
            return userView;

        // Chama metodo para criar a carteira do usuário
        var walletService = new WalletService(_context);
        var wallet = await walletService.CreateWallet(user.Id);

        if (wallet.HasErrors)
            return new UserViewModel { Errors = wallet.Errors };

        return userView;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => string.Equals(x.Email, email));
    }

    private async Task<UserViewModel> SaveChanges(User user)
    {
        UserViewModel userViewModel = new();
        try
        {
            if (user.Id == 0)
                _context.Users.Add(user);
            else
            {
                var existingUser = await _context.Users.FindAsync(user.Id);

                if (existingUser == null)
                    return new UserViewModel
                    {
                        Errors = new List<string> { "Usuário não encontrado para atualização." }
                    };

                _context.Entry(existingUser).CurrentValues.SetValues(user);
            }

            await _context.SaveChangesAsync();
            userViewModel = new UserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
        catch (Exception ex)
        {
            userViewModel.Errors = new List<string> { ex.Message };
        }

        return userViewModel;
    }
}
