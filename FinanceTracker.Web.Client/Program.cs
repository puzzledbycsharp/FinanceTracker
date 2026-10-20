using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FinanceTracker.Shared.Services;
using FinanceTracker.Web.Client.Services;
using FinanceTracker.Shared.Data;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add device-specific services used by the FinanceTracker.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// 1. Register the SQLite Database for the WebAssembly sandbox
builder.Services.AddScoped(sp => new FinanceDbContext("finance.db"));

// 2. Register Shared and Client-Side Services
builder.Services.AddScoped<CategoryMemoryService>();

// Add HttpClient to talk to the server
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress!) });

// If you have a CSV parser service class, register it here too:
// builder.Services.AddScoped<BankCsvParserService>(); 
// Or if it uses an interface:
builder.Services.AddScoped<ICsvParserService, BankCsvParserService>();

builder.Services.AddScoped<FinancialInsightService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<GamificationEngineService>();

await builder.Build().RunAsync();