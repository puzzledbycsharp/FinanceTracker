using Microsoft.Extensions.Logging;
using FinanceTracker.Shared.Services;
using FinanceTracker.Services;
using FinanceTracker.Shared.Data;
using FinanceTracker.Web.Services;

namespace FinanceTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddScoped<PdfExtractionService>();

        builder.Services.AddScoped<CategoryMemoryService>();



        // Add device-specific services used by the FinanceTracker.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        // 1. Setup Local SQLite Database Context
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "finance.db");
        builder.Services.AddSingleton(new FinanceDbContext(dbPath));

        // 2. Register your Google Auth and Sync Services
        // This maps the interface to the mobile-specific login flow
        builder.Services.AddSingleton<IGoogleAuthService, MauiGoogleAuthService>();

        builder.Services.AddScoped(sp =>
        {
            var authService = sp.GetRequiredService<IGoogleAuthService>();
            return new GoogleDriveSyncService(authService, dbPath);
        });


        builder.Services.AddScoped<ICsvParserService, BankCsvParserService>();

        builder.Services.AddScoped<FinancialInsightService>();
        
        builder.Services.AddScoped<TransactionService>();

        builder.Services.AddScoped<ToastService>();

        builder.Services.AddScoped<GamificationEngineService>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}