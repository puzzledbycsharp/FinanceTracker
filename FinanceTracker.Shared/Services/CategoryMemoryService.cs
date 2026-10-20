using FinanceTracker.Shared.Data;
using FinanceTracker.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FinanceTracker.Shared.Services
{
    public class CategoryMemoryService
    {
        private readonly FinanceDbContext _dbContext;

        public CategoryMemoryService(FinanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Phase A: Smart Matching. Runs automatically during CSV/PDF Ingestion.
        /// </summary>
        public async Task ApplyCategoriesAsync(List<Transaction> transactions)
        {
            // Load all learned rules from SQLite
            var rules = await _dbContext.CategorizationRules.ToListAsync();

            foreach (var tx in transactions)
            {
                // Skip items that were already strictly categorized by the parser (like Transfers)
                if (!string.IsNullOrEmpty(tx.Category) && tx.Category != "Uncategorized")
                    continue;

                tx.CleanedDescription = CleanDescription(tx.OriginalDescription);

                // Find the first rule where the rule's keyword exists anywhere in the cleaned description
                var matchedRule = rules.FirstOrDefault(r =>
                    tx.CleanedDescription.Contains(r.MatchKeyword, StringComparison.OrdinalIgnoreCase) ||
                    tx.OriginalDescription.Contains(r.MatchKeyword, StringComparison.OrdinalIgnoreCase));

                if (matchedRule != null)
                {
                    tx.Category = matchedRule.Category;
                }
                else
                {
                    tx.Category = "Uncategorized";
                }
            }
        }

        /// <summary>
        /// Phase B: User Learning. Triggered when a user edits a transaction category in the UI.
        /// </summary>
        public async Task LearnCategoryAsync(string rawDescription, string newCategory)
        {
            // We use the cleaned description as the base for the new rule
            string keyword = CleanDescription(rawDescription);
            if (string.IsNullOrWhiteSpace(keyword)) return;

            // Check if we already have a rule for this keyword
            var existingRule = await _dbContext.CategorizationRules
                .FirstOrDefaultAsync(r => r.MatchKeyword.ToUpper() == keyword.ToUpper());

            if (existingRule != null)
            {
                // Update the memory if the user changed their mind
                existingRule.Category = newCategory;
                _dbContext.CategorizationRules.Update(existingRule);
            }
            else
            {
                // Create a brand new memory rule
                var newRule = new CategorizationRule
                {
                    MatchKeyword = keyword,
                    Category = newCategory
                };
                await _dbContext.CategorizationRules.AddAsync(newRule);
            }

            // Save changes to SQLite
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Helper: Strips out store numbers, dates, and special characters common in bank exports.
        /// </summary>
        private string CleanDescription(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "UNKNOWN";

            // Remove numbers (like store IDs), pound signs, and asterisks
            var cleaned = Regex.Replace(raw, @"[\d#\*]", "").Trim();

            // Remove multiple spaces left behind by the regex
            cleaned = Regex.Replace(cleaned, @"\s+", " ");

            // If stripping numbers ruined the string (e.g. "7-11"), fall back to the raw string
            return string.IsNullOrWhiteSpace(cleaned) || cleaned.Length < 3 ? raw.Trim() : cleaned;
        }



    }
}