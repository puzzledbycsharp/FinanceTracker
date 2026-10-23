using Microsoft.EntityFrameworkCore;
using FinanceTracker.Shared.Models;
using FinanceTracker.Shared.Data;

namespace FinanceTracker.Shared.Services
{
    public class TransactionStateService
    {
        private readonly FinanceDbContext _dbContext;
        private List<Transaction>? _cache;

        // This event allows pages to instantly refresh if data is modified elsewhere
        public event Action? OnStateChanged;

        public TransactionStateService(FinanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Only hits the database if the cache is completely empty
        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            if (_cache == null)
            {
                _cache = await _dbContext.Transactions
                    .OrderByDescending(t => t.Date)
                    .ThenBy(t => t.Id)
                    .ToListAsync();
            }
            return _cache;
        }

        public bool IsLoaded => _cache != null;

        // Write-Through Cache: Update memory instantly, persist to SQLite
        public async Task UpdateCategoryAsync(Transaction txn, string newCategory)
        {
            // 1. Update Database
            txn.Category = newCategory;
            _dbContext.Transactions.Update(txn);
            await _dbContext.SaveChangesAsync();

            // 2. Notify any connected UI components to instantly re-render
            NotifyStateChanged();
        }

        public async Task AddSplitTransactionsAsync(Transaction original, List<Transaction> children)
        {
            // 1. Update Database
            original.IsSplit = true;
            original.Category = "Split";
            _dbContext.Transactions.Update(original);
            await _dbContext.Transactions.AddRangeAsync(children);
            await _dbContext.SaveChangesAsync();

            // 2. Update Memory Cache Instantly
            if (_cache != null)
            {
                _cache.InsertRange(0, children);
                _cache = _cache.OrderByDescending(t => t.Date).ThenBy(t => t.Id).ToList();
            }

            NotifyStateChanged();
        }

        public async Task ForceRefreshAsync()
        {
            _cache = null;
            await GetTransactionsAsync();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnStateChanged?.Invoke();
    }
}