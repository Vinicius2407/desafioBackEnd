using Engine.Interfaces;
using Engine.Singleton;
using Models.DTOs.Transaction;
using Models.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Services;
public class TransactionService : IService
{
    private readonly DBApp _context;
    public TransactionService(DBApp context)
    {
        _context = context;
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
