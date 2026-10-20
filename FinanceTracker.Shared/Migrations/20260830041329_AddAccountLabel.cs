using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinanceTracker.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategorizationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MatchKeyword = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorizationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OriginalDescription = table.Column<string>(type: "TEXT", nullable: false),
                    CleanedDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Institution = table.Column<string>(type: "TEXT", nullable: false),
                    Account = table.Column<string>(type: "TEXT", nullable: false),
                    UniqueHash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CategorizationRules",
                columns: new[] { "Id", "Category", "MatchKeyword" },
                values: new object[,]
                {
                    { 1, "Groceries", "WALMART" },
                    { 2, "Groceries", "WAL-MART" },
                    { 3, "Groceries", "TARGET" },
                    { 4, "Groceries", "WHOLE FOODS" },
                    { 5, "Groceries", "WHOLEFD" },
                    { 6, "Groceries", "TRADER JOE" },
                    { 7, "Groceries", "ALDI" },
                    { 8, "Groceries", "KROGER" },
                    { 9, "Groceries", "SAFEWAY" },
                    { 10, "Groceries", "GIANT" },
                    { 11, "Groceries", "WEGMANS" },
                    { 12, "Groceries", "COSTCO WHOLESALE" },
                    { 13, "Groceries", "SAMS CLUB" },
                    { 14, "Groceries", "FOOD LION" },
                    { 15, "Groceries", "H MART" },
                    { 16, "Groceries", "LOTTE" },
                    { 17, "Groceries", "AMAZON RETAI" },
                    { 20, "Dining & Coffee", "STARBUCKS" },
                    { 21, "Dining & Coffee", "DUNKIN" },
                    { 22, "Dining & Coffee", "MCDONALD" },
                    { 23, "Dining & Coffee", "CHIPOTLE" },
                    { 24, "Dining & Coffee", "PANERA" },
                    { 25, "Dining & Coffee", "CHICK-FIL-A" },
                    { 26, "Dining & Coffee", "SWEETGREEN" },
                    { 27, "Dining & Coffee", "DOORDASH" },
                    { 28, "Dining & Coffee", "UBER EATS" },
                    { 29, "Dining & Coffee", "GRUBHUB" },
                    { 30, "Dining & Coffee", "POSTMATES" },
                    { 31, "Dining & Coffee", "YOYA SUSHI" },
                    { 32, "Dining & Coffee", "GREAT COOKIE" },
                    { 33, "Dining & Coffee", "KFTEA" },
                    { 34, "Dining & Coffee", "SNOBALLS" },
                    { 40, "Gas & Transit", "EXXON" },
                    { 41, "Gas & Transit", "MOBIL" },
                    { 42, "Gas & Transit", "SHELL" },
                    { 43, "Gas & Transit", "BP" },
                    { 44, "Gas & Transit", "CHEVRON" },
                    { 45, "Gas & Transit", "WAWA" },
                    { 46, "Gas & Transit", "ROYAL FARMS" },
                    { 47, "Gas & Transit", "UBER" },
                    { 48, "Gas & Transit", "LYFT" },
                    { 49, "Gas & Transit", "EZ PASS" },
                    { 50, "Gas & Transit", "PARKMOBILE" },
                    { 51, "Gas & Transit", "DEPENDABLE AUTO" },
                    { 52, "Gas & Transit", "COSTCO GAS" },
                    { 60, "Utilities & Bills", "COMCAST" },
                    { 61, "Utilities & Bills", "XFINITY" },
                    { 62, "Utilities & Bills", "VERIZON" },
                    { 63, "Utilities & Bills", "AT&T" },
                    { 64, "Utilities & Bills", "T-MOBILE" },
                    { 65, "Utilities & Bills", "BGE" },
                    { 66, "Utilities & Bills", "PEPCO" },
                    { 67, "Utilities & Bills", "DOMINION" },
                    { 68, "Utilities & Bills", "WASTE MANAGEMENT" },
                    { 69, "Utilities & Bills", "ANNEARUNDELCO" },
                    { 80, "Subscriptions & Streaming", "NETFLIX" },
                    { 81, "Subscriptions & Streaming", "HULU" },
                    { 82, "Subscriptions & Streaming", "DISNEY PLUS" },
                    { 83, "Subscriptions & Streaming", "SPOTIFY" },
                    { 84, "Subscriptions & Streaming", "APPLE MUSIC" },
                    { 85, "Subscriptions & Streaming", "AMAZON PRIME" },
                    { 86, "Subscriptions & Streaming", "YOUTUBE PREMIUM" },
                    { 87, "Subscriptions & Streaming", "HBO MAX" },
                    { 88, "Subscriptions & Streaming", "PARAMOUNT" },
                    { 89, "Subscriptions & Streaming", "AUDIBLE" },
                    { 100, "AI & Software Tools", "OPENAI" },
                    { 101, "AI & Software Tools", "CHATGPT" },
                    { 102, "AI & Software Tools", "ANTHROPIC" },
                    { 103, "AI & Software Tools", "MIDJOURNEY" },
                    { 104, "AI & Software Tools", "ELEVENLABS" },
                    { 105, "AI & Software Tools", "GITHUB" },
                    { 106, "AI & Software Tools", "JETBRAINS" },
                    { 107, "AI & Software Tools", "ADOBE" },
                    { 108, "AI & Software Tools", "BLACKMAGIC" },
                    { 109, "AI & Software Tools", "CANVA" },
                    { 110, "AI & Software Tools", "BLUEHOST" },
                    { 111, "AI & Software Tools", "NAME-CHEAP" },
                    { 120, "Shopping", "AMZN MKTP" },
                    { 121, "Shopping", "BEST BUY" },
                    { 122, "Shopping", "MICRO CENTER" },
                    { 123, "Shopping", "B&H PHOTO" },
                    { 124, "Shopping", "HOME DEPOT" },
                    { 125, "Shopping", "LOWES" },
                    { 126, "Shopping", "EBAY" },
                    { 127, "Shopping", "ETSY" },
                    { 128, "Shopping", "IKEA" },
                    { 129, "Shopping", "SEPHORA" },
                    { 130, "Shopping", "AMAZON.COM" },
                    { 140, "Health & Fitness", "CVS" },
                    { 141, "Health & Fitness", "WALGREENS" },
                    { 142, "Health & Fitness", "PLANET FITNESS" },
                    { 143, "Health & Fitness", "LA FITNESS" },
                    { 144, "Health & Fitness", "LIFETIME FITNESS" },
                    { 145, "Health & Fitness", "ROGUE FITNESS" },
                    { 146, "Health & Fitness", "BARE PERFORMANCE" },
                    { 147, "Health & Fitness", "QUEST DIAGNOSTICS" },
                    { 148, "Health & Fitness", "PEDIATRIC" },
                    { 149, "Health & Fitness", "UNIVERSITY OF MARYLA" },
                    { 160, "Income", "PAYROLL" },
                    { 161, "Income", "DIRECT DEP" },
                    { 162, "Income", "DIR DEP" },
                    { 163, "Income", "INTEREST PAYMENT" },
                    { 164, "Income", "DIVIDEND" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategorizationRules");

            migrationBuilder.DropTable(
                name: "Transactions");
        }
    }
}
