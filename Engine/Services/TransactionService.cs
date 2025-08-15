using Engine.Interfaces;
using Engine.Singleton;
using Microsoft.EntityFrameworkCore;
using Models.DTOs.Bet;
using Models.DTOs.Transaction;
using Models.Model;
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
