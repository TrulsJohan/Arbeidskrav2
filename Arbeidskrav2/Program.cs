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

                string? choice;

                if (!market.Auth.IsLoggedIn)
                {
                    Console.WriteLine("1. Register new account");
                    Console.WriteLine("2. Log in");
                    Console.WriteLine("0. Exit");
                }
                else
                {
                    Console.WriteLine("1. View my profile");
                    Console.WriteLine("2. Create new listing");
                    Console.WriteLine("3. Browse available items      [coming soon]");
                    Console.WriteLine("4. Log out");
                    Console.WriteLine("5. View my listings");
                    Console.WriteLine("6. Buy / purchase an item       [coming soon]");
                    Console.WriteLine("0. Exit");
                }

                Console.Write("\nYour choice: ");
                choice = Console.ReadLine()?.Trim();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            if (!market.Auth.IsLoggedIn)
                                RegisterFlow(market.Auth);
                            else
                                ShowProfile(market.Auth.CurrentUser!);
                            break;

                        case "2":
                            if (!market.Auth.IsLoggedIn)
                                LoginFlow(market.Auth);
                            else
                                CreateListingFlow(market);
                            break;

                        case "3":
                            if (market.Auth.IsLoggedIn)
                                Console.WriteLine("\nBrowse items feature – to be implemented next.");
                            else
                                Console.WriteLine("\nPlease log in first.");
                            break;

                        case "4":
                            if (market.Auth.IsLoggedIn)
                                market.Auth.Logout();
                            break;

                        case "5":
                            if (market.Auth.IsLoggedIn)
                                ShowMyListings(market.Auth.CurrentUser!);
                            else
                                Console.WriteLine("\nPlease log in first.");
                            break;

                        case "6":
                            if (market.Auth.IsLoggedIn)
                                Console.WriteLine("\nPurchase feature – to be implemented next.");
                            else
                                Console.WriteLine("\nPlease log in first.");
                            break;

                        case "0":
                            Console.WriteLine("\nThank you for using Second-Hand Market. Goodbye!");
                            return;

                        default:
                            Console.WriteLine("\nInvalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        private static void PrintHeader(AuthService auth)
        {
            Console.WriteLine("=== Second-Hand Market ===");
            Console.WriteLine(auth.IsLoggedIn
                ? $"Logged in as: {auth.CurrentUsername}"
                : "Not logged in");
            Console.WriteLine(new string('-', 40));
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

        private static void ShowProfile(User user)
        {
            Console.WriteLine($"\n=== Profile: {user.Username} ===");
            Console.WriteLine($"Member since: {user.RegisteredAt:yyyy-MM-dd}");
            Console.WriteLine($"Active listings: {user.ActiveListings.Count}");
            Console.WriteLine($"Sold items:      {user.SoldListings.Count}");
            Console.WriteLine($"Purchases:       {user.Purchases.Count}");
            Console.WriteLine($"Reviews received: {user.ReceivedReviews.Count}");
            Console.WriteLine();
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

        private static void ShowMyListings(User user)
        {
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
        }
    }
}