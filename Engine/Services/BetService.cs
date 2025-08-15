
using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.Bet;
using Models.DTOs.User;
using Models.Helpers;
using Models.Model;

namespace Engine.Services;
public class BetService : IService
{
    private readonly DBApp _context;
    public BetService(DBApp context)
    {
        _context = context;
    }

    public async Task<BetViewModel> PlaceBetAsync(CreateBetDto createBetDto)
    {
        var betViewModel = new BetViewModel();
        using (var transactionContext = await _context.Database.BeginTransactionAsync())
        {
            var userService = new UserService(_context);
            var user = await userService.GetUserByIdAsync(createBetDto.UserId);

            if (user == null)
                return new BetViewModel { Errors = new List<string> { "Usuário não encontrado." } };

            var walletService = new WalletService(_context);
            var wallet = await walletService.GetWalletByUserIdAsync(createBetDto.UserId);

            if (wallet.HasErrors)
                return new BetViewModel { Errors = wallet.Errors };

            if (wallet.BalanceAvailable < createBetDto.Amount)
                return new BetViewModel { Errors = new List<string> { "Saldo insuficiente." } };

            try
            {
                var bet = new Bet
                {
                    UserId = createBetDto.UserId,
                    Amount = createBetDto.Amount,
                    Status = Enumerators.BetStatus.PENDING,
                    CreatedAt = DateTime.UtcNow,
                    PrizeAmount = 0,
                };

                betViewModel = await SaveChangesAsync(bet);

                if (betViewModel.HasErrors)
                    throw new Exception();

                var transactionService = new TransactionService(_context);
                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Amount = createBetDto.Amount,
                    Type = Enumerators.TransactionType.BET,
                    CreatedAt = DateTime.UtcNow,
                };

                var transactionViewModel = await transactionService.CreateTransactionAsync(transaction);
                if (transactionViewModel.HasErrors)
                {
                    betViewModel.Errors = transactionViewModel.Errors;
                    throw new Exception();
                }

                wallet.BalanceAvailable -= createBetDto.Amount;
                wallet.BalanceBlocked += createBetDto.Amount;

                wallet = await walletService.UpdateWalletAsync(wallet);

                if (wallet.HasErrors)
                {
                    betViewModel.Errors = wallet.Errors;
                    throw new Exception();
                }

                await _context.SaveChangesAsync();
                await transactionContext.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!betViewModel.HasErrors)
                    betViewModel.Errors = new List<string> { "A aposta não pôde ser processada. Outro processo modificou o saldo antes." };
                await transactionContext.RollbackAsync();
            }
            catch (Exception ex)
            {
                if (!betViewModel.HasErrors)
                    betViewModel.Errors = new List<string> { "Erro ao processar a aposta.", ex.Message };
                await transactionContext.RollbackAsync();
            }
        }
        return betViewModel;
    }

    private async Task<BetViewModel> SaveChangesAsync(Bet bet)
    {
        BetViewModel betViewModel = new();
        try
        {
            if (bet.Id == 0)
                _context.Bets.Add(bet);
            else
            {
                var existingBet = await _context.Users.FindAsync(bet.Id);

                if (existingBet == null)
                    return new BetViewModel
                    {
                        Errors = new List<string> { "Usuário não encontrado para atualização." }
                    };

                _context.Entry(existingBet).CurrentValues.SetValues(bet);
            }

            await _context.SaveChangesAsync();
            betViewModel = new BetViewModel
            {
                Id = bet.Id,
                Status = bet.Status,
                Amout = bet.Amount,
                CreatedAt = bet.CreatedAt,
                PrizeAmount = bet.PrizeAmount,
            };
        }
        catch (Exception ex)
        {
            betViewModel.Errors = new List<string> { ex.Message };
        }

        return betViewModel;
    }
}
