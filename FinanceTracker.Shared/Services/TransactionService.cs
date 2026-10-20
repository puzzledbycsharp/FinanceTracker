using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Shared.Data;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared.Services
{
    public class TransactionService
    {
        private readonly FinanceDbContext _dbContext;

        public TransactionService(FinanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a single transaction (e.g., from the manual Add UI) and checks for conflicts against imports.
        /// </summary>
        public async Task AddTransactionAsync(Transaction newTxn)
        {
            await CheckForAndFlagConflictsAsync(newTxn);

            await _dbContext.Transactions.AddAsync(newTxn);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Processes a bulk list of transactions (e.g., from a CSV import).
        /// </summary>
        public async Task ImportTransactionsAsync(IEnumerable<Transaction> importedTxns)
        {
            foreach (var txn in importedTxns)
            {
                // Enforce that these are marked as imports
                txn.Source = "Import";
                await CheckForAndFlagConflictsAsync(txn);
                await _dbContext.Transactions.AddAsync(txn);
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Core logic to find Date + Amount matches and flag them if the Sources differ.
        /// </summary>
        private async Task CheckForAndFlagConflictsAsync(Transaction newTxn)
        {
            // Look for a potential duplicate based on exact Date and Amount
            var potentialMatch = await _dbContext.Transactions
                .FirstOrDefaultAsync(t => t.Date.Date == newTxn.Date.Date
                                       && t.Amount == newTxn.Amount
                                       && !t.IsConflict
                                       && !t.IsSplit);

            // Cross-Source Conflict Rule: Only flag if one is Manual and the other is Import
            if (potentialMatch != null && potentialMatch.Source != newTxn.Source)
            {
                // Flag both as conflicting and link them together
                potentialMatch.IsConflict = true;
                potentialMatch.ConflictingTransactionId = newTxn.Id;

                newTxn.IsConflict = true;
                newTxn.ConflictingTransactionId = potentialMatch.Id;

                _dbContext.Transactions.Update(potentialMatch);
            }
        }
    }
}