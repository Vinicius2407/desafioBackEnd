
using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.Bet;
using Models.DTOs.Transaction;
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

    public async Task<BetViewModel> GetFullBetByIdAsync(long betId)
    {
        var bet = await _context.Bets
                                .Include(bet => bet.User)
                                .Include(bet => bet.Transactions)
                                .FirstOrDefaultAsync(bet => bet.Id == betId);

        if (bet == null)
            return new BetViewModel { Errors = new List<string> { "Aposta não encontrada." } };

        var betViewModel = new BetViewModel
        {
            Id = bet.Id,
            UserId = bet.UserId,
            Amount = bet.Amount,
            PrizeAmount = bet.PrizeAmount,
            Status = bet.Status,
            CreatedAt = bet.CreatedAt,
            Transactions = bet.Transactions.Select(transaction =>
                new TransactionViewModel
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Type = transaction.Type,
                    CreatedAt = transaction.CreatedAt,
                    Description = transaction.Description,
                }
            ).ToList(),
        };

        return betViewModel;
    }

    public async Task<List<BetViewModel>> GetBetsByUserIdAsync(long userId)
    {
        var betsViewModel = new List<BetViewModel>();

        var bets = await _context.Bets.Where(x => x.UserId == userId).ToListAsync();

        if (!bets.Any())
            return betsViewModel;

        betsViewModel = bets.Select(bet =>
            new BetViewModel
            {
                Id = bet.Id,
                UserId = bet.UserId,
                Amount = bet.Amount,
                CreatedAt = bet.CreatedAt,
                Status = bet.Status,
                PrizeAmount = bet.PrizeAmount
            }
        ).ToList();

        return betsViewModel;
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

    public async Task<BetViewModel> ProcessBetResultAsync(long betId)
    {
        var betViewModel = await GetFullBetByIdAsync(betId);
        using (var transactionContext = await _context.Database.BeginTransactionAsync())
        {
            try
            {

                if (betViewModel.HasErrors)
                    return betViewModel;

                var user = await new UserService(_context).GetFullUserByIdAsync(betViewModel.UserId);

                if (user.HasErrors)
                {
                    betViewModel.Errors = user.Errors;
                    return betViewModel;
                }

                user.Bets = await GetBetsByUserIdAsync(user.Id);

                var walletService = new WalletService(_context);
                var wallet = await walletService.GetWalletByUserIdAsync(user.Id);

                if (wallet.HasErrors)
                {
                    betViewModel.Errors = wallet.Errors;
                    throw new Exception();
                }

                var win = Helpers.WinRandomHelper.WinOrLose();
                var statusBet = win ? Enumerators.BetStatus.WON : Enumerators.BetStatus.LOST;
                betViewModel = await UpdateBetStatusAsync(betId, statusBet);

                if (betViewModel.HasErrors)
                    throw new Exception();

                if (win)
                {
                    var transaction = new Transaction
                    {
                        Amount = (betViewModel.Amount + (user.IsOnLosingStreak ? betViewModel.Amount * 0.1M : 0)) * 2,
                        CreatedAt = DateTime.UtcNow,
                        Type = Enumerators.TransactionType.WIN,
                        BetId = betViewModel.Id,
                        WalletId = user.Wallet.Id,
                        Description = $"Aposta vencedora{(user.IsOnLosingStreak ? ", 10% de bonus." : "")}",
                    };

                    var transactionService = new TransactionService(_context);
                    var transactionViewModel = await transactionService.CreateTransactionAsync(transaction);

                    if (transactionViewModel.HasErrors)
                    {
                        betViewModel.Errors = transactionViewModel.Errors;
                        throw new Exception();
                    }

                    wallet.BalanceAvailable += transaction.Amount;
                }

                wallet.BalanceBlocked -= betViewModel.Amount;
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
                await transactionContext.RollbackAsync();
                return new BetViewModel { Errors = new List<string> { "A aposta não pôde ser processada. Outro processo modificou o saldo antes." } };
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

    public async Task<BetViewModel> UpdateBetStatusAsync(long betId, Enumerators.BetStatus betStatus)
    {
        var bet = await _context.Bets.FindAsync(betId);
        bet!.Status = betStatus;

        var betViewModel = await SaveChangesAsync(bet);
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
                Amount = bet.Amount,
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
