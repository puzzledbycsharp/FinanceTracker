using FinanceTracker.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Tabula;
using Tabula.Extractors;
using UglyToad.PdfPig;

namespace FinanceTracker.Web.Services
{
    public class PdfExtractionService
    {
        private readonly ILogger<PdfExtractionService> _logger;

        public PdfExtractionService(ILogger<PdfExtractionService> logger)
        {
            _logger = logger;
        }

        public List<Transaction> ExtractTransactionsFromPdf(Stream pdfStream, string institution)
        {
            var transactions = new List<Transaction>();
            bool isMandT = institution.ToUpper() == "M&T";

            if (isMandT)
            {
                using (PdfDocument document = PdfDocument.Open(pdfStream, new ParsingOptions() { ClipPaths = true }))
                {
                    for (int pageIndex = 1; pageIndex <= document.NumberOfPages; pageIndex++)
                    {
                        PageArea page = ObjectExtractor.Extract(document, pageIndex);
                        IExtractionAlgorithm algorithm = new SpreadsheetExtractionAlgorithm();
                        IReadOnlyList<Table> tables = algorithm.Extract(page);

                        foreach (var table in tables)
                        {
                            foreach (var row in table.Rows)
                            {
                                var rowCells = new List<string>();
                                foreach (var cell in row)
                                {
                                    rowCells.Add(cell.GetText().Trim());
                                }

                                var transaction = MapRowToTransaction(rowCells, "M&T");
                                if (transaction != null)
                                {
                                    transaction.UniqueHash = GenerateHash(transaction);
                                    transactions.Add(transaction);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                transactions = ExtractChaseStatementFlexible(pdfStream);
            }

            return transactions;
        }

        private List<Transaction> ExtractChaseStatementFlexible(Stream pdfStream)
        {
            var transactions = new List<Transaction>();

            using (PdfDocument document = PdfDocument.Open(pdfStream))
            {
                _logger.LogInformation($"[PDF Parser] Opened Chase PDF. Total Pages: {document.NumberOfPages}");

                int pageNumber = 1;
                foreach (var page in document.GetPages())
                {
                    string text = page.Text;
                    _logger.LogInformation($"[PDF Parser] Processing Page {pageNumber}, Text Length: {text.Length}");

                    var matches = Regex.Matches(text, @"(\d{2}/\d{2})\s*(.+?)(-?\$?\d{1,3}(?:,\d{3})*\.\d{2})");

                    _logger.LogInformation($"[PDF Parser] Found {matches.Count} potential transaction matches on Page {pageNumber}");

                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            string dateStr = match.Groups[1].Value;
                            string description = match.Groups[2].Value.Trim();
                            string rawAmountStr = match.Groups[3].Value.Replace("$", "").Replace(",", "");

                            if (DateTime.TryParse(dateStr, out DateTime parsedDate) && decimal.TryParse(rawAmountStr, out decimal amount))
                            {
                                // Skip summary artifacts, headers, and metadata lines
                                // Inside ExtractChaseStatementFlexible, update your skip filter check to this:
                                if (description.Contains("New Balance") || description.Contains("Page ") ||
                                    description.Contains("Statement Date") || description.Contains("Balance Subject") ||
                                    description.Contains("Credit Access Line") || description.Contains("Previous points balance") ||
                                    description.StartsWith("/")) // <-- Ignores partial slash fragments like "/26"
                                    continue;

                                description = Regex.Replace(description, @"Order Number.*", "").Trim();
                                description = Regex.Replace(description, @"\s+", " ");

                                string type = "Expense";
                                string category = "Uncategorized";

                                // Route credit card payment lines to Transfer so they don't skew cash flow
                                if (description.Contains("Payment Thank You") || description.Contains("CHASE CREDIT CRD EPAY") || description.Contains("WEB XFER"))
                                {
                                    type = "Transfer";
                                    category = "Transfer";
                                }
                                else if (amount < 0)
                                {
                                    type = "Income";
                                }

                                var txn = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Institution = "Chase",
                                    Date = new DateTime(DateTime.Now.Year, parsedDate.Month, parsedDate.Day),
                                    OriginalDescription = description,
                                    CleanedDescription = description,
                                    Amount = Math.Abs(amount),
                                    Type = type,
                                    Category = category
                                };

                                txn.UniqueHash = GenerateHash(txn);

                                if (!transactions.Exists(t => t.UniqueHash == txn.UniqueHash))
                                {
                                    transactions.Add(txn);
                                    _logger.LogInformation($"[Extracted]: {txn.Date:MM/dd} | {txn.OriginalDescription} | {txn.Amount:C} | {txn.Type}");
                                }
                            }
                        }
                    }
                    pageNumber++;
                }
            }

            _logger.LogInformation($"[PDF Parser Finished] Total transactions extracted from Chase PDF: {transactions.Count}");
            return transactions;
        }

        private Transaction MapRowToTransaction(List<string> cells, string institution)
        {
            try
            {
                if (institution.ToUpper() == "M&T")
                {
                    if (cells.Count < 5) return null;
                    if (!DateTime.TryParse(cells[1], out DateTime date)) return null;

                    string description = cells[2];
                    string debitStr = cells[3];
                    string creditStr = cells[4];

                    decimal amount = 0;
                    string type = "Expense";
                    string category = "Uncategorized";

                    if (decimal.TryParse(debitStr, out decimal debit) && debit != 0)
                    {
                        amount = debit;
                        type = "Expense";

                        // Handle checking-side credit card payments
                        if (description.ToUpper().Contains("CHASE CREDIT CRD EPAY"))
                        {
                            type = "Transfer";
                            category = "Transfer";
                        }
                    }
                    else if (decimal.TryParse(creditStr, out decimal credit) && credit != 0)
                    {
                        amount = credit;
                        type = "Income";
                    }
                    else
                    {
                        return null;
                    }

                    return new Transaction
                    {
                        Id = Guid.NewGuid(),
                        Institution = "M&T",
                        Date = date,
                        OriginalDescription = description,
                        CleanedDescription = description,
                        Amount = Math.Abs(amount),
                        Type = type,
                        Category = category
                    };
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private string GenerateHash(Transaction t)
        {
            string rawData = $"{t.Date:yyyyMMdd}_{t.Amount}_{t.OriginalDescription}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }
    }
}