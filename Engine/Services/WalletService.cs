using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.Model;

namespace Engine.Services;
public class WalletService : IService
{
    private readonly DBApp _context;
    public WalletService(DBApp context)
    {
        _context = context;
    }

    public async Task<Wallet> GetWalletByUserIdAsync(long userId)
    {
        var wallet = await _context.Wallets.Include(x => x.Currency).FirstOrDefaultAsync(x => x.UserId == userId);
        if (wallet == null)
            return new Wallet { Errors = new List<string> { "Carteira não encontrada para o usuário." } };
        return wallet;
    }

    public async Task<Wallet> CreateWalletAsync(long userId)
    {
        Wallet wallet = new();
        var user = await _context.Users.FindAsync(userId) ?? throw new ArgumentException("Usuário não encontrado.", nameof(userId));

        if (user.Wallet != null)
            wallet.Errors = new List<string> { "Usuário já possui uma carteira." };

        wallet = new Wallet
        {
            BalanceAvailable = 1000,
            BalanceBlocked = 0,
            CurrencyId = _context.Currencies.FirstOrDefault(x => string.Equals(x.Code, "BRL"))!.Id,
            UserId = userId,
        };

        return await SaveChangesAsync(wallet);
    }

    private async Task<Wallet> SaveChangesAsync(Wallet wallet)
    {
        Wallet walletResponse = new();
        try
        {
            if (wallet.Id == 0)
                _context.Wallets.Add(wallet);
            else
            {
                var existingWallet = await _context.Wallets.FindAsync(wallet.Id) ?? throw new InvalidOperationException("Carteira não encontrada para atualização.");
                existingWallet.CurrencyId = wallet.CurrencyId;
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            walletResponse.Errors = new List<string> { ex.Message };
        }

        return walletResponse;
    }
}
