using System.Text.Json;
using FinanceTracker.DataGenerator;

Console.WriteLine("Generating 6 months of demo transactions via Bogus...");

var transactions = DemoDataSeeder.Generate(6);

// Serialize and save directly to the Web project
string jsonResult = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
string outputPath = @"..\..\..\..\FinanceTracker.Web\DemoSeedData.json";

File.WriteAllText(outputPath, jsonResult);

Console.WriteLine($"Successfully saved {transactions.Count} transactions to {outputPath}");