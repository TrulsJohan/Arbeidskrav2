using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbeidskrav2
{
    class Program
    {
        static void Main()
        {
            var market = new SecondHandMarket();
            while (true)
            {
                Console.Clear();
                PrintHeader(market.Auth);

                if (!market.Auth.IsLoggedIn)
                {
                    // Guest Menu
                    Console.WriteLine("1. Register new account");
                    Console.WriteLine("2. Login");
                    Console.WriteLine("0. Exit application");
                }
                else
                {
                    // Logged-in Menu
                    Console.WriteLine("1. View my profile");
                    Console.WriteLine("2. Create new listing");
                    Console.WriteLine("3. Browse & search listings");
                    Console.WriteLine("4. Buy an item (by ID - backup)");
                    Console.WriteLine("5. View my listings");
                    Console.WriteLine("6. View transaction history");
                    Console.WriteLine("7. Leave a review for a purchase");
                    Console.WriteLine("8. Logout");
                    Console.WriteLine("0. Exit application");
                }

                Console.Write("\nEnter your choice: ");
                var choice = Console.ReadLine()?.Trim();

                try
                {
                    if (!market.Auth.IsLoggedIn)
                    {
                        // Guest choices
                        switch (choice)
                        {
                            case "1": RegisterFlow(market.Auth); break;
                            case "2": LoginFlow(market.Auth); break;
                            case "0": 
                                Console.WriteLine("\nThank you for using Second-Hand Market. Goodbye!");
                                return;
                            default: 
                                Console.WriteLine("Invalid choice. Please try again."); 
                                break;
                        }
                    }
                    else
                    {
                        // Logged-in choices
                        switch (choice)
                        {
                            case "1": ShowProfile(market.Auth.CurrentUser!, market); break;
                            case "2": CreateListingFlow(market); break;
                            case "3": BrowseListingsFlow(market); break;
                            case "4": BuyItemFlow(market); break;
                            case "5": ShowMyListings(market); break;
                            case "6": ShowTransactionHistoryFlow(market); break;
                            case "7": LeaveReviewFlow(market); break;
                            case "8": market.Auth.Logout(); break;
                            case "0":
                                Console.WriteLine("\nThank you for using Second-Hand Market. Goodbye!");
                                return;
                            default:
                                Console.WriteLine("Invalid choice. Please try again.");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }

                if (choice != "0")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }

        private static void PrintHeader(AuthService auth)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                SECOND-HAND MARKET                          ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
    
            if (auth.IsLoggedIn)
                Console.WriteLine($"║  Logged in as: @{auth.CurrentUsername,-35}         ║");
            else
                Console.WriteLine("║  Not logged in                                             ║");
    
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }

        private static void RegisterFlow(AuthService auth)
        {
            Console.Write("\nChoose username: ");
            var username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Password: ");
            var password = Console.ReadLine() ?? "";

            try
            {
                auth.Register(username, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nRegistration failed: {ex.Message}");
            }
        }

        private static void LoginFlow(AuthService auth)
        {
            Console.Write("\nUsername: ");
            var username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Password: ");
            var password = Console.ReadLine() ?? "";

            if (!auth.Login(username, password))
                Console.WriteLine("\nLogin failed – incorrect username or password.");
        }

        private static void ShowProfile(User user, SecondHandMarket market)
        {
            var avgRating = market.GetAverageRating(user);

            Console.WriteLine($"\n=== Profile: @{user.Username} ===");
            Console.WriteLine($"Member since:     {user.RegisteredAt:yyyy-MM-dd}");
            Console.WriteLine($"Active listings:  {user.ActiveListings.Count}");
            Console.WriteLine($"Purchases:        {user.Purchases.Count}");
            Console.WriteLine($"Reviews received: {user.ReceivedReviews.Count}");

            if (avgRating.HasValue)
                Console.WriteLine($"Average rating:   {avgRating.Value}/6 ★");
            else
                Console.WriteLine("Average rating:   No reviews yet");

            if (user.ReceivedReviews.Any())
            {
                Console.WriteLine("\nRecent reviews:");
                foreach (var review in user.ReceivedReviews.OrderByDescending(r => r.ReviewedAt).Take(3))
                {
                    Console.WriteLine($"  • {review.Rating}/6 from @{review.Reviewer.Username}: {review.Comment ?? "No comment"}");
                }
            }
        }

        private static void CreateListingFlow(SecondHandMarket market)
        {
            var user = market.Auth.CurrentUser!;
            Console.WriteLine("\n=== Create New Listing ===");

            Console.Write("Title: ");
            var title = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Description: ");
            var description = Console.ReadLine()?.Trim() ?? "";

            Console.WriteLine("\nCategories:");
            var categories = Enum.GetValues(typeof(Category)).Cast<Category>().ToList();
            for (int i = 0; i < categories.Count; i++)
                Console.WriteLine($"  {i + 1}. {categories[i]}");
            Console.Write("Choose category (number): ");
            if (!int.TryParse(Console.ReadLine(), out int catChoice) || catChoice < 1 || catChoice > categories.Count)
            {
                Console.WriteLine("Invalid category selection.");
                return;
            }
            var category = categories[catChoice - 1];

            Console.WriteLine("\nConditions:");
            var conditions = Enum.GetValues(typeof(Condition)).Cast<Condition>().ToList();
            for (int i = 0; i < conditions.Count; i++)
                Console.WriteLine($"  {i + 1}. {conditions[i]}");
            Console.Write("Choose condition (number): ");
            if (!int.TryParse(Console.ReadLine(), out int condChoice) || condChoice < 1 || condChoice > conditions.Count)
            {
                Console.WriteLine("Invalid condition selection.");
                return;
            }
            var condition = conditions[condChoice - 1];

            Console.Write("Price (NOK): ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price <= 0)
            {
                Console.WriteLine("Invalid price.");
                return;
            }

            try
            {
                market.CreateListing(user, title, description, category, condition, price);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFailed to create listing: {ex.Message}");
            }
        }

        private static void ShowMyListings(SecondHandMarket market)
        {
            var user = market.Auth.CurrentUser!;
            Console.WriteLine($"\n=== Your Listings ({user.Listings.Count} total) ===");

            if (!user.Listings.Any())
            {
                Console.WriteLine("You have no listings yet.");
                return;
            }

            Console.WriteLine("Active:");
            var active = user.ActiveListings;
            if (active.Any())
            {
                foreach (var listing in active.OrderByDescending(l => l.CreatedAt))
                {
                    Console.WriteLine($"  • {listing.ShortInfo()}  ID: {listing.Id.ToString()[..8]}...");
                }
            }
            else
            {
                Console.WriteLine("  No active listings.");
            }

            Console.WriteLine("\nSold:");
            var sold = user.SoldListings;
            if (sold.Any())
            {
                foreach (var listing in sold.OrderByDescending(l => l.SoldAt))
                {
                    Console.WriteLine($"  • {listing.ShortInfo()}  Sold on {listing.SoldAt:yyyy-MM-dd}");
                }
            }
            else
            {
                Console.WriteLine("  No sold items yet.");
            }
            
            Console.WriteLine("\nTo delete an active listing, enter its ID (or press Enter to skip): ");
            var delInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(delInput) && Guid.TryParse(delInput, out Guid delId))
            {
                market.DeleteListing(delId);
            }
        }

        private static void BrowseListingsFlow(SecondHandMarket market)
        {
            Console.WriteLine("\n=== Browse & Search Listings ===\n");

            Category? selectedCategory = null;
            string? keyword = null;

            Console.Write("Filter by category? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "y")
            {
                var cats = Enum.GetValues(typeof(Category)).Cast<Category>().ToList();
                for (int i = 0; i < cats.Count; i++)
                    Console.WriteLine($"  {i + 1}. {cats[i]}");
                Console.Write("Choose category number (or Enter to skip): ");
                if (int.TryParse(Console.ReadLine(), out int c) && c >= 1 && c <= cats.Count)
                    selectedCategory = cats[c - 1];
            }

            Console.Write("Search by keyword (or press Enter to skip): ");
            keyword = Console.ReadLine()?.Trim();

            var results = market.GetFilteredListings(selectedCategory, keyword);

            if (!results.Any())
            {
                Console.WriteLine("No listings found.");
                return;
            }
            
            market.PrintNumberedListings(results);

            Console.Write("\nSelect a listing to view (0 to go back): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > results.Count)
                return;

            var selected = results[choice - 1];
            
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine($"=== {selected.Title} ===");
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"Seller:      @{selected.Seller.Username}");
            Console.WriteLine($"Category:    {selected.CategoryDisplay}");
            Console.WriteLine($"Condition:   {selected.ConditionDisplay}");
            Console.WriteLine($"Price:       {selected.Price:N0} NOK");
            Console.WriteLine($"Created:     {selected.CreatedAt:yyyy-MM-dd}");
            Console.WriteLine($"Description: {selected.Description}");
            Console.WriteLine(new string('=', 70));

            Console.WriteLine("\n1. Buy this item");
            Console.WriteLine("2. Go back");
            Console.Write("Select an option: ");

            if (Console.ReadLine()?.Trim() == "1")
            {
                try
                {
                    var transaction = market.PurchaseListing(selected.Id, market.Auth.CurrentUser!);
                    if (transaction != null)
                    {
                        Console.Write("\nWould you like to leave a review for the seller? (Y/N): ");
                        if (Console.ReadLine()?.Trim().ToUpper() == "Y")
                        {
                            Console.Write("Rating (1-6): ");
                            if (int.TryParse(Console.ReadLine(), out int r) && r >= 1 && r <= 6)
                            {
                                Console.Write("Comment (optional, press Enter to skip): ");
                                var comment = Console.ReadLine()?.Trim();
                                market.LeaveReview(market.Auth.CurrentUser!, transaction.Id, r, comment);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Purchase failed: {ex.Message}");
                }
            }
        }

        private static void ShowTransactionHistoryFlow(SecondHandMarket market)
        {
            var user = market.Auth.CurrentUser!;
    
            Console.WriteLine("\n=== Transaction History ===");
            Console.WriteLine($"Logged in as: @{user.Username}\n");

            Console.WriteLine("─ Items You Have Bought ─");
            if (user.Purchases.Any())
            {
                foreach (var t in user.Purchases.OrderByDescending(t => t.PurchasedAt))
                {
                    Console.WriteLine($"• {t.Listing.Title}");
                    Console.WriteLine($"  Price:     {t.Price:N0} NOK");
                    Console.WriteLine($"  Seller:    @{t.Seller.Username}");
                    Console.WriteLine($"  Date:      {t.PurchasedAt:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"  Listing ID:{t.Listing.Id.ToString()[..8]}...");
                    Console.WriteLine(new string('-', 50));
                }
            }
            else
            {
                Console.WriteLine("  You haven't bought anything yet.");
            }
            
            Console.WriteLine("\n─ Items You Have Sold ─");
            var soldTransactions = market.GetSoldTransactions(user);

            if (soldTransactions.Any())
            {
                foreach (var t in soldTransactions.OrderByDescending(t => t.PurchasedAt))
                {
                    Console.WriteLine($"• {t.Listing.Title}");
                    Console.WriteLine($"  Price:     {t.Price:N0} NOK");
                    Console.WriteLine($"  Buyer:     @{t.Buyer.Username}");
                    Console.WriteLine($"  Date:      {t.PurchasedAt:yyyy-MM-dd HH:mm}");
                    Console.WriteLine($"  Listing ID:{t.Listing.Id.ToString()[..8]}...");
                    Console.WriteLine(new string('-', 50));
                }
            }
            else
            {
                Console.WriteLine("  You haven't sold anything yet.");
            }
        }
        
        private static void LeaveReviewFlow(SecondHandMarket market)
        {
            var buyer = market.Auth.CurrentUser!;
            Console.WriteLine("\n=== Leave a Review ===");
            if (!buyer.Purchases.Any())
            {
                Console.WriteLine("You have no purchases yet.");
                return;
            }

            Console.WriteLine("Your recent purchases:");
            for (int i = 0; i < buyer.Purchases.Count; i++)
            {
                var t = buyer.Purchases[i];
                Console.WriteLine($"{i + 1}. {t.Listing.Title} - {t.Price:N0} NOK (ID: {t.Id.ToString()[..8]}...)");
            }

            Console.Write("\nEnter the number of the purchase you want to review: ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > buyer.Purchases.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var selectedTransaction = buyer.Purchases[choice - 1];

            Console.Write("\nRating (1-6): ");
            if (!int.TryParse(Console.ReadLine(), out int rating) || rating < 1 || rating > 6)
            {
                Console.WriteLine("Rating must be between 1 and 6.");
                return;
            }

            Console.Write("Comment (optional, press Enter to skip): ");
            var comment = Console.ReadLine()?.Trim();

            try
            {
                market.LeaveReview(buyer, selectedTransaction.Id, rating, comment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Review failed: {ex.Message}");
            }
        }
        
        private static void BuyItemFlow(SecondHandMarket market)
        {
            Console.WriteLine("\n=== Buy by ID (backup option) ===");
            Console.Write("Enter full Listing ID: ");
            if (Guid.TryParse(Console.ReadLine()?.Trim(), out Guid id))
            {
                try
                {
                    market.PurchaseListing(id, market.Auth.CurrentUser!);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Purchase failed: {ex.Message}");
                }
            }
        }
    }
}