using FinanceTracker.Shared.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FinanceTracker.Shared.Services
{
    public interface ICsvParserService
    {
        Task<List<Transaction>> ParseCsvAsync(Stream fileStream, string institution);
    }
}