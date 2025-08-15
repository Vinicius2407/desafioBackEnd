
using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.Bet;
using Models.DTOs.Transaction;
using Models.Helpers;
using Models.Model;
using X.PagedList;
using X.PagedList.Extensions;

namespace Engine.Services;
public class BetService : IService
{
    private readonly DBApp _context;
    public BetService(DBApp context)
    {
        _context = context;
    }

    public async Task<BetViewModel> GetBetByIdAsync(long betId)
    {
        var bet = await _context.Bets.FindAsync(betId);

        if (bet == null)
            return new BetViewModel { Errors = new List<string> { "Aposta não encontrada" } };

        var betViewModel = new BetViewModel
        {
            Id = bet.Id,
            Amount = bet.Amount,
            Status = bet.Status,
            PrizeAmount = bet.PrizeAmount,
            CreatedAt = bet.CreatedAt,
            UserId = bet.UserId
        };

        return betViewModel;
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

    public IPagedList<BetViewModel> GetBetsPaginedByUserId(long? userId, int page, int itemsPerPage)
    {
        var query = _context.Bets.Include(x => x.Transactions).OrderByDescending(x => x.CreatedAt).AsQueryable();

        if (userId != null)
            query = query.Where(x => x.UserId == userId);

        var result = query
                        .Select(x => new BetViewModel
                        {
                            Id = x.Id,
                            Amount = x.Amount,
                            Status = x.Status,
                            PrizeAmount = x.PrizeAmount,
                            CreatedAt = x.CreatedAt,
                            UserId = x.UserId,
                            Transactions = x.Transactions.Select(t => new TransactionViewModel
                            {
                                Id = t.Id,
                                Amount = t.Amount,
                                Type = t.Type,
                                CreatedAt = t.CreatedAt,
                                Description = t.Description,
                                BetId = t.BetId,
                            }).ToList(),
                        })
                        .ToPagedList(page, itemsPerPage);
        return result;
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
                    BetId = betViewModel.Id,
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
            catch (DbUpdateConcurrencyException)
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
                var transactionService = new TransactionService(_context);
                var userService = new UserService(_context);
                var walletService = new WalletService(_context);


                if (betViewModel.HasErrors)
                    return betViewModel;

                var user = await userService.GetFullUserByIdAsync(betViewModel.UserId);

                if (user.HasErrors)
                {
                    betViewModel.Errors = user.Errors;
                    return betViewModel;
                }

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

                Transaction transaction = null!;
                if (win)
                {
                    betViewModel.PrizeAmount = betViewModel.Amount * 2;
                    transaction = new Transaction
                    {
                        Amount = (decimal)betViewModel.PrizeAmount,
                        Type = Enumerators.TransactionType.WIN,
                        BetId = betViewModel.Id,
                        WalletId = user.Wallet.Id,
                        Description = "Aposta vencedora",
                    };

                    var transactionViewModel = await transactionService.CreateTransactionAsync(transaction);
                    if (transactionViewModel.HasErrors)
                    {
                        betViewModel.Errors = transactionViewModel.Errors;
                        throw new Exception();
                    }

                    wallet.BalanceAvailable += transaction.Amount;
                    user = await userService.ResetLoseStreak(user.Id);
                    var bet = _context.Bets.Find(betViewModel.Id);
                    bet!.PrizeAmount = betViewModel.PrizeAmount;
                }
                else
                {
                    user = await userService.IncrementLoser(user.Id);
                    user.Bets = await GetBetsByUserIdAsync(user.Id);
                    if (user.IsOnLosingStreak())
                    {
                        var transactionBonus = new Transaction
                        {
                            Amount = user.LostBetAmountPercentage,
                            Type = Enumerators.TransactionType.REFUND,
                            WalletId = user.Wallet.Id,
                            Description = "Bônus por sequência de derrotas.",
                        };

                        var transactionBonusViewModel = await transactionService.CreateTransactionAsync(transactionBonus);
                        if (transactionBonusViewModel.HasErrors)
                        {
                            betViewModel.Errors = transactionBonusViewModel.Errors;
                            throw new Exception();
                        }

                        wallet.BalanceAvailable += transactionBonus.Amount;
                    }
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
            catch (DbUpdateConcurrencyException)
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

        if (betStatus == Enumerators.BetStatus.CANCELLED)
        {
            var wallet = await _context.Users.Include(x => x.Wallet).Where(x => x.Id == bet.UserId).Select(x => x.Wallet).FirstOrDefaultAsync();

            if (wallet == null)
                return new BetViewModel { Errors = new List<string> { "Carteira não encontrada para executar o estorno." } };

            wallet.BalanceAvailable += bet.Amount;
            wallet.BalanceBlocked -= bet.Amount;
        }

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
                var existingBet = await _context.Bets.FindAsync(bet.Id);

                if (existingBet == null)
                    return new BetViewModel
                    {
                        Errors = new List<string> { "Aposta não encontrado para atualização." }
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
