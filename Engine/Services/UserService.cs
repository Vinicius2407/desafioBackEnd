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

    public async Task<UserViewModel> GetUserByIdAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new UserViewModel
            {
                Errors = new List<string> { "Usuário não encontrado." }
            };

        return new UserViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    public async Task<FullUserView> GetFullUserByIdAsync(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new FullUserView { Errors = new List<string> { "Usuario não encontrado" } };

        var walletService = new WalletService(_context);
        var wallet = await walletService.GetWalletByUserIdAsync(userId);

        if (wallet.HasErrors)
            return new FullUserView { Errors = wallet.Errors };

        var userFull = new FullUserView
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Document = user.Document,
            PhoneNumber = user.PhoneNumber,
            LoseStreakCounter = user.LoseStreakCounter,
            Wallet = new Models.DTOs.Wallet.WalletViewModel
            {
                Id = wallet.Id,
                BalanceAvailable = wallet.BalanceAvailable,
                BalanceBlocked = wallet.BalanceBlocked,
                Currency = new Models.DTOs.Currency.CurrencyViewModel
                {
                    Id = wallet.Currency.Id,
                    Name = wallet.Currency.Name,
                    Code = wallet.Currency.Code,
                    Symbol = wallet.Currency.Symbol
                },
            },
        };

        return userFull;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => string.Equals(x.Email, email));
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
        var wallet = await walletService.CreateWalletAsync(user.Id);

        if (wallet.HasErrors)
            return new UserViewModel { Errors = wallet.Errors };

        return userView;
    }

    public async Task<FullUserView> IncrementLoser(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        user.LoseStreakCounter += 1;
        var userFull = await SaveChanges(user);

        if (userFull.HasErrors)
            return new FullUserView { Errors = userFull.Errors };

        return await GetFullUserByIdAsync(userId);
    }

    public async Task<FullUserView> ResetLoseStreak(long userId)
    {
        var user = await _context.Users.FindAsync(userId);
        user.LoseStreakCounter = 0;
        var userFull = await SaveChanges(user);

        if (userFull.HasErrors)
            return new FullUserView { Errors = userFull.Errors };

        return await GetFullUserByIdAsync(userId);
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
