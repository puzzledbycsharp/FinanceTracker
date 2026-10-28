using FinanceTracker.Shared.Data;
using FinanceTracker.Shared.Models;
using FinanceTracker.Shared.Services;
using FinanceTracker.Web.Components;
using FinanceTracker.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the FinanceTracker.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

// 1. Setup Local SQLite Database Context
// Read the flag from appsettings.json
bool isDemoMode = builder.Configuration.GetValue<bool>("EnvironmentSettings:IsDemoMode");

// Route to the correct database file
string dbFileName = isDemoMode ? "finance-test.db" : "finance.db";
string dbPath = Path.Combine(builder.Environment.ContentRootPath, dbFileName);
builder.Services.AddScoped(sp => new FinanceDbContext(dbPath));

// Core Application Services
builder.Services.AddScoped<FinanceTracker.Web.Services.PdfExtractionService>();
builder.Services.AddScoped<CategoryMemoryService>();
builder.Services.AddScoped<ICsvParserService, BankCsvParserService>();
builder.Services.AddScoped<FinancialInsightService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<GamificationEngineService>();
builder.Services.AddScoped<TransactionStateService>();

// 2. Configure HttpClient (Required by the new GoogleDriveSyncService)
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

// 3. Register the new Google Drive Sync Service
// (Removed the old IGoogleAuthService duplicate registration)
builder.Services.AddScoped<FinanceTracker.Shared.Services.GoogleDriveSyncService>();

/* 
// TEMPORARILY DISABLED: 
// We are using a hardcoded token in GoogleDriveSyncService for testing. 
// Uncomment this later when we implement the full dynamic login UI.
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.ClientId = "YOUR_WEB_CLIENT_ID.apps.googleusercontent.com";
    options.ProviderOptions.Authority = "https://accounts.google.com/";
    options.ProviderOptions.ResponseType = "id_token token";
    options.ProviderOptions.DefaultScopes.Add("https://www.googleapis.com/auth/drive.appdata");
});
builder.Services.AddAuthorization();
*/

var app = builder.Build();

// ==========================================
// DEMO MODE: AUTO-SEEDING LOGIC
// ==========================================
if (isDemoMode)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

    // 1. Ensure the finance-test.db file actually exists
    db.Database.EnsureCreated();

    // 2. Check if the database has any transactions generated for today
    var today = DateTime.Today;
    bool hasTodayData = db.Transactions.Any(t => t.Date >= today);

    // 3. If today's data is missing, the demo database is stale. Rebuild it.
    if (!hasTodayData)
    {
        app.Logger.LogInformation("Demo database is stale or empty. Regenerating 6 months of data...");

        // Wipe existing stale demo data
        db.Transactions.RemoveRange(db.Transactions);
        db.SaveChanges();

        // Generate a fresh 6-month batch up to today's exact date
        var freshDemoData = FinanceTracker.Shared.DemoDataSeeder.Generate(6);

        // Inject the fresh data
        db.Transactions.AddRange(freshDemoData);
        db.SaveChanges();

        app.Logger.LogInformation($"Successfully seeded {freshDemoData.Count} demo transactions.");

        // Seed 6 months of Budgets
        var demoBudgets = FinanceTracker.Shared.DemoDataSeeder.GenerateBudgets(6);
        db.Budgets.AddRange(demoBudgets);
        db.SaveChanges();

        // Seed 6 months of Achievements
        var demoAchievements = FinanceTracker.Shared.DemoDataSeeder.GenerateAchievements(6);
        db.EarnedAchievements.AddRange(demoAchievements);

        // Ensure the profile's TotalPoints reflects the seeded demo achievements
        var totalDemoPoints = demoAchievements.Sum(a => a.PointsAwarded);
        var profile = db.GamificationProfiles.FirstOrDefault();
        if (profile != null)
        {
            profile.TotalPoints += totalDemoPoints;
        }
        else
        {
            db.GamificationProfiles.Add(new GamificationProfile { Id = 1, TotalPoints = totalDemoPoints });
        }

        db.SaveChanges();
    }
    else
    {
        app.Logger.LogInformation("Demo database is already up to date for today. Skipping generation.");
    }
}
// ==========================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(FinanceTracker.Shared._Imports).Assembly,
        typeof(FinanceTracker.Web.Client._Imports).Assembly);

app.MapPost("/api/extract-pdf", async (
    [Microsoft.AspNetCore.Mvc.FromForm] IFormFile file,
    [Microsoft.AspNetCore.Mvc.FromServices] FinanceTracker.Web.Services.PdfExtractionService pdfExtractor,
    [Microsoft.AspNetCore.Mvc.FromServices] ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation($"[API] Received PDF file upload: {file.FileName}, Size: {file.Length} bytes");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        string institution = file.FileName.Contains("Chase", StringComparison.OrdinalIgnoreCase) ? "Chase" : "M&T";

        logger.LogInformation($"[API] Calling ExtractTransactionsFromPdf for institution: {institution}");
        var transactions = pdfExtractor.ExtractTransactionsFromPdf(memoryStream, institution);

        logger.LogInformation($"[API] Successfully extracted {transactions.Count} transactions from PDF.");
        return Results.Ok(transactions);
    }
    catch (Exception ex)
    {
        logger.LogError($"[API EXCEPTION CRASH]: {ex.GetType().Name} - {ex.Message} \n{ex.StackTrace}");
        return Results.Problem(ex.Message);
    }
}).DisableAntiforgery();

app.Run();