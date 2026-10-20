using System.Collections.Generic;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared.Data
{
    public static class SeedData
    {
        public static List<CategorizationRule> GetDefaultRules()
        {
            return new List<CategorizationRule>
            {
                // ==========================================
                // 1. GROCERIES & SUPERMARKETS
                // ==========================================
                new CategorizationRule { Id = 1, MatchKeyword = "WALMART", Category = "Groceries" },
                new CategorizationRule { Id = 2, MatchKeyword = "WAL-MART", Category = "Groceries" },
                new CategorizationRule { Id = 3, MatchKeyword = "TARGET", Category = "Groceries" },
                new CategorizationRule { Id = 4, MatchKeyword = "WHOLE FOODS", Category = "Groceries" },
                new CategorizationRule { Id = 5, MatchKeyword = "WHOLEFD", Category = "Groceries" },
                new CategorizationRule { Id = 6, MatchKeyword = "TRADER JOE", Category = "Groceries" },
                new CategorizationRule { Id = 7, MatchKeyword = "ALDI", Category = "Groceries" },
                new CategorizationRule { Id = 8, MatchKeyword = "KROGER", Category = "Groceries" },
                new CategorizationRule { Id = 9, MatchKeyword = "SAFEWAY", Category = "Groceries" },
                new CategorizationRule { Id = 10, MatchKeyword = "GIANT", Category = "Groceries" },
                new CategorizationRule { Id = 11, MatchKeyword = "WEGMANS", Category = "Groceries" },
                new CategorizationRule { Id = 12, MatchKeyword = "COSTCO WHOLESALE", Category = "Groceries" },
                new CategorizationRule { Id = 13, MatchKeyword = "SAMS CLUB", Category = "Groceries" },
                new CategorizationRule { Id = 14, MatchKeyword = "FOOD LION", Category = "Groceries" },
                new CategorizationRule { Id = 15, MatchKeyword = "H MART", Category = "Groceries" },
                new CategorizationRule { Id = 16, MatchKeyword = "LOTTE", Category = "Groceries" },
                new CategorizationRule { Id = 17, MatchKeyword = "AMAZON RETAI", Category = "Groceries" }, // Amazon Fresh/Groceries

                // ==========================================
                // 2. DINING, COFFEE & DELIVERY
                // ==========================================
                new CategorizationRule { Id = 20, MatchKeyword = "STARBUCKS", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 21, MatchKeyword = "DUNKIN", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 22, MatchKeyword = "MCDONALD", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 23, MatchKeyword = "CHIPOTLE", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 24, MatchKeyword = "PANERA", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 25, MatchKeyword = "CHICK-FIL-A", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 26, MatchKeyword = "SWEETGREEN", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 27, MatchKeyword = "DOORDASH", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 28, MatchKeyword = "UBER EATS", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 29, MatchKeyword = "GRUBHUB", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 30, MatchKeyword = "POSTMATES", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 31, MatchKeyword = "YOYA SUSHI", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 32, MatchKeyword = "GREAT COOKIE", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 33, MatchKeyword = "KFTEA", Category = "Dining & Coffee" },
                new CategorizationRule { Id = 34, MatchKeyword = "SNOBALLS", Category = "Dining & Coffee" },

                // ==========================================
                // 3. GAS, AUTOMOTIVE & TRANSIT
                // ==========================================
                new CategorizationRule { Id = 40, MatchKeyword = "EXXON", Category = "Gas & Transit" },
                new CategorizationRule { Id = 41, MatchKeyword = "MOBIL", Category = "Gas & Transit" },
                new CategorizationRule { Id = 42, MatchKeyword = "SHELL", Category = "Gas & Transit" },
                new CategorizationRule { Id = 43, MatchKeyword = "BP", Category = "Gas & Transit" },
                new CategorizationRule { Id = 44, MatchKeyword = "CHEVRON", Category = "Gas & Transit" },
                new CategorizationRule { Id = 45, MatchKeyword = "WAWA", Category = "Gas & Transit" },
                new CategorizationRule { Id = 46, MatchKeyword = "ROYAL FARMS", Category = "Gas & Transit" },
                new CategorizationRule { Id = 47, MatchKeyword = "UBER", Category = "Gas & Transit" },
                new CategorizationRule { Id = 48, MatchKeyword = "LYFT", Category = "Gas & Transit" },
                new CategorizationRule { Id = 49, MatchKeyword = "EZ PASS", Category = "Gas & Transit" },
                new CategorizationRule { Id = 50, MatchKeyword = "PARKMOBILE", Category = "Gas & Transit" },
                new CategorizationRule { Id = 51, MatchKeyword = "DEPENDABLE AUTO", Category = "Gas & Transit" },
                new CategorizationRule { Id = 52, MatchKeyword = "COSTCO GAS", Category = "Gas & Transit" },

                // ==========================================
                // 4. UTILITIES, HOUSING & INTERNET
                // ==========================================
                new CategorizationRule { Id = 60, MatchKeyword = "COMCAST", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 61, MatchKeyword = "XFINITY", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 62, MatchKeyword = "VERIZON", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 63, MatchKeyword = "AT&T", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 64, MatchKeyword = "T-MOBILE", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 65, MatchKeyword = "BGE", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 66, MatchKeyword = "PEPCO", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 67, MatchKeyword = "DOMINION", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 68, MatchKeyword = "WASTE MANAGEMENT", Category = "Utilities & Bills" },
                new CategorizationRule { Id = 69, MatchKeyword = "ANNEARUNDELCO", Category = "Utilities & Bills" },

                // ==========================================
                // 5. DIGITAL MEDIA, ENTERTAINMENT & SUBSCRIPTIONS
                // ==========================================
                new CategorizationRule { Id = 80, MatchKeyword = "NETFLIX", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 81, MatchKeyword = "HULU", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 82, MatchKeyword = "DISNEY PLUS", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 83, MatchKeyword = "SPOTIFY", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 84, MatchKeyword = "APPLE MUSIC", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 85, MatchKeyword = "AMAZON PRIME", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 86, MatchKeyword = "YOUTUBE PREMIUM", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 87, MatchKeyword = "HBO MAX", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 88, MatchKeyword = "PARAMOUNT", Category = "Subscriptions & Streaming" },
                new CategorizationRule { Id = 89, MatchKeyword = "AUDIBLE", Category = "Subscriptions & Streaming" },

                // ==========================================
                // 6. SOFTWARE, AI & CREATIVE TOOLS
                // ==========================================
                new CategorizationRule { Id = 100, MatchKeyword = "OPENAI", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 101, MatchKeyword = "CHATGPT", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 102, MatchKeyword = "ANTHROPIC", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 103, MatchKeyword = "MIDJOURNEY", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 104, MatchKeyword = "ELEVENLABS", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 105, MatchKeyword = "GITHUB", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 106, MatchKeyword = "JETBRAINS", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 107, MatchKeyword = "ADOBE", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 108, MatchKeyword = "BLACKMAGIC", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 109, MatchKeyword = "CANVA", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 110, MatchKeyword = "BLUEHOST", Category = "AI & Software Tools" },
                new CategorizationRule { Id = 111, MatchKeyword = "NAME-CHEAP", Category = "AI & Software Tools" },

                // ==========================================
                // 7. SHOPPING & HARDWARE
                // ==========================================
                new CategorizationRule { Id = 120, MatchKeyword = "AMZN MKTP", Category = "Shopping" },
                new CategorizationRule { Id = 121, MatchKeyword = "BEST BUY", Category = "Shopping" },
                new CategorizationRule { Id = 122, MatchKeyword = "MICRO CENTER", Category = "Shopping" },
                new CategorizationRule { Id = 123, MatchKeyword = "B&H PHOTO", Category = "Shopping" },
                new CategorizationRule { Id = 124, MatchKeyword = "HOME DEPOT", Category = "Shopping" },
                new CategorizationRule { Id = 125, MatchKeyword = "LOWES", Category = "Shopping" },
                new CategorizationRule { Id = 126, MatchKeyword = "EBAY", Category = "Shopping" },
                new CategorizationRule { Id = 127, MatchKeyword = "ETSY", Category = "Shopping" },
                new CategorizationRule { Id = 128, MatchKeyword = "IKEA", Category = "Shopping" },
                new CategorizationRule { Id = 129, MatchKeyword = "SEPHORA", Category = "Shopping" },
                new CategorizationRule { Id = 130, MatchKeyword = "AMAZON.COM", Category = "Shopping" },

                // ==========================================
                // 8. FITNESS, HEALTH & MEDICAL
                // ==========================================
                new CategorizationRule { Id = 140, MatchKeyword = "CVS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 141, MatchKeyword = "WALGREENS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 142, MatchKeyword = "PLANET FITNESS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 143, MatchKeyword = "LA FITNESS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 144, MatchKeyword = "LIFETIME FITNESS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 145, MatchKeyword = "ROGUE FITNESS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 146, MatchKeyword = "BARE PERFORMANCE", Category = "Health & Fitness" },
                new CategorizationRule { Id = 147, MatchKeyword = "QUEST DIAGNOSTICS", Category = "Health & Fitness" },
                new CategorizationRule { Id = 148, MatchKeyword = "PEDIATRIC", Category = "Health & Fitness" },
                new CategorizationRule { Id = 149, MatchKeyword = "UNIVERSITY OF MARYLA", Category = "Health & Fitness" },

                // ==========================================
                // 9. INCOME & DIRECT DEPOSITS
                // ==========================================
                new CategorizationRule { Id = 160, MatchKeyword = "PAYROLL", Category = "Income" },
                new CategorizationRule { Id = 161, MatchKeyword = "DIRECT DEP", Category = "Income" },
                new CategorizationRule { Id = 162, MatchKeyword = "DIR DEP", Category = "Income" },
                new CategorizationRule { Id = 163, MatchKeyword = "INTEREST PAYMENT", Category = "Income" },
                new CategorizationRule { Id = 164, MatchKeyword = "DIVIDEND", Category = "Income" }
            };
        }
    }
}