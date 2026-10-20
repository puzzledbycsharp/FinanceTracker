using FinanceTracker.Shared.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTracker.Shared.Services
{
    public class BankCsvParserService : ICsvParserService
    {
        public async Task<List<Transaction>> ParseCsvAsync(Stream fileStream, string institution)
        {
            var transactions = new List<Transaction>();
            bool isChase = institution.ToUpper() == "CHASE";

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Chase has headers, M&T does not
                HasHeaderRecord = isChase,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, config);

            if (isChase)
            {
                await csv.ReadAsync();
                csv.ReadHeader();
            }

            while (await csv.ReadAsync())
            {
                try
                {
                    var transaction = isChase ? ParseChaseRow(csv) : ParseMandTRow(csv);

                    if (transaction != null)
                    {
                        transaction.UniqueHash = GenerateHash(transaction);
                        transactions.Add(transaction);
                    }
                }
                catch
                {
                    // Catch and skip empty lines or malformed rows at the end of the file
                    continue;
                }
            }

            return transactions;
        }

        private Transaction ParseChaseRow(CsvReader csv)
        {
            var dateStr = csv.GetField("Posting Date") ?? csv.GetField("Transaction Date");
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            if (!DateTime.TryParse(dateStr, out DateTime date)) return null;

            var amount = csv.GetField<decimal>("Amount");
            var description = csv.GetField("Description");

            return new Transaction
            {
                Id = Guid.NewGuid(),
                Institution = "Chase",
                Date = date,
                OriginalDescription = description?.Trim(),
                CleanedDescription = description?.Trim(),
                Amount = Math.Abs(amount),
                Type = amount < 0 ? "Expense" : "Income"
            };
        }

        private Transaction ParseMandTRow(CsvReader csv)
        {
            // M&T Layout based on visual extraction: 
            // [0] Account Number, [1] Date, [2] Description, [3] Debit, [4] Credit, [5] Balance

            var dateStr = csv.GetField(1);
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            if (!DateTime.TryParse(dateStr, out DateTime date)) return null;

            var description = csv.GetField(2);
            var debitStr = csv.GetField(3);
            var creditStr = csv.GetField(4);

            decimal amount = 0;
            string type = "";

            // Check if there is any non-zero value in the Debit column, regardless of negative signs
            if (decimal.TryParse(debitStr, out decimal debit) && debit != 0)
            {
                amount = debit;
                type = "Expense";
            }
            // Check if there is any non-zero value in the Credit column
            else if (decimal.TryParse(creditStr, out decimal credit) && credit != 0)
            {
                amount = credit;
                type = "Income";
            }
            else
            {
                // Both columns are empty or strictly zero
                return null;
            }
            if (description.ToUpper().Contains("PAYMENT THANK YOU") || description.ToUpper().Contains("CHASE CREDIT CRD EPAY") || description.ToUpper().Contains("WEB XFER"))
            {
                type = "Transfer";
            }
            return new Transaction
            {
                Id = Guid.NewGuid(),
                Institution = "M&T",
                Date = date,
                OriginalDescription = description?.Trim(),
                CleanedDescription = description?.Trim(),
                Amount = Math.Abs(amount), // Strips any negative signs the bank included
                Type = type
            };
        }

        private string GenerateHash(Transaction t)
        {
            string rawData = $"{t.Date:yyyyMMdd}_{t.Amount}_{t.OriginalDescription}";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }
    }
}