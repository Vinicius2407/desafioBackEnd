using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.Bet;
using Models.DTOs.Transaction;
using Models.Model;
using Models.Helpers;
using X.PagedList;
using X.PagedList.Extensions;

namespace Engine.Services;
public class TransactionService : IService
{
    private readonly DBApp _context;
    public TransactionService(DBApp context)
    {
        _context = context;
    }

    public IPagedList<TransactionViewModel> GetTransactionsPaginedByWalletId(long userId, int page, int itemsPerPage)
    {
        var walletId = _context.Wallets
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .FirstOrDefault();
        var query = _context.Transactions.Include(x => x.Bet).Where(x => x.WalletId == walletId).OrderByDescending(x => x.CreatedAt);
        var result = query
                        .Select(x => new TransactionViewModel
                        {
                            Id = x.Id,
                            Amount = x.Amount,
                            Type = x.Type,
                            CreatedAt = x.CreatedAt,
                            BetId = x.BetId,
                            Bet = x.Bet != null
                                            ? new BetViewModel
                                            {
                                                Id = x.Bet.Id,
                                                Amount = x.Bet.Amount,
                                                CreatedAt = x.Bet.CreatedAt,
                                                Status = x.Bet.Status,
                                                UserId = x.Bet.UserId,
                                                PrizeAmount = x.Bet.PrizeAmount,
                                            }
                                            : null
                        })
                        .ToPagedList(page, itemsPerPage);
        return result;
    }

    public async Task<TransactionViewModel> CreateTransactionAsync(Transaction createTransaction)
    {
        if (createTransaction == null)
            return new TransactionViewModel { Errors = new List<string> { "Impossivel criar objeto null" } };

        return await SaveChangesAsync(createTransaction);
    }

    public async Task<TransactionViewModel> CreateMovementAsync(CreateTransactionDto createTransactionDto)
    {
        var validationList = new List<Enumerators.TransactionType> { Enumerators.TransactionType.DEPOSIT, Enumerators.TransactionType.WITHDRAW };

        if (!validationList.Contains(createTransactionDto.Type))
            return new TransactionViewModel { Errors = new List<string> { "Tipo de transação inválida. Deve ser um depósito ou uma retirada." } };

        var wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.UserId == createTransactionDto.UserId);
        if (wallet == null)
            return new TransactionViewModel { Errors = new List<string> { "Carteira não encontrada para o usuário." } };

        if (createTransactionDto.Type == Enumerators.TransactionType.WITHDRAW && createTransactionDto.Amount > wallet.BalanceAvailable)
            return new TransactionViewModel { Errors = new List<string> { "Saldo insuficiente para realizar o saque." } };

        wallet.BalanceAvailable += createTransactionDto.Type == Enumerators.TransactionType.DEPOSIT ? createTransactionDto.Amount : -createTransactionDto.Amount;
        var transaction = new Transaction
        {
            Amount = createTransactionDto.Amount,
            Type = createTransactionDto.Type,
            WalletId = wallet.Id,
            Description = createTransactionDto.Type == Enumerators.TransactionType.DEPOSIT ? "Depósito na carteira" : "Saque de valor na carteira",
        };

        return await SaveChangesAsync(transaction);
    }

    private async Task<TransactionViewModel> SaveChangesAsync(Transaction transaction)
    {
        var transactionViewModel = new TransactionViewModel();
        try
        {
            transaction.Bet = null;
            transaction.Wallet = null!;

            if (transaction.Id == 0)
                _context.Transactions.Add(transaction);
            else
            {
                var existingTransaction = await _context.Transactions.FindAsync(transaction.Id)
                    ?? throw new InvalidOperationException("Transação não encontrada para atualização.");

                _context.Entry(existingTransaction).CurrentValues.SetValues(transaction);
            }
            await _context.SaveChangesAsync();
            transactionViewModel = new TransactionViewModel
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Type = transaction.Type,
                CreatedAt = transaction.CreatedAt,
            };
        }
        catch (Exception ex)
        {
            transactionViewModel.Errors = new List<string> { ex.Message };
        }

        return transactionViewModel;
    }
}
