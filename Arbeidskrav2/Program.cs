namespace Arbeidskrav2;

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
                Console.WriteLine("1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("0. Exit");
            }
            else
            {
                Console.WriteLine("1. View my profile");
                Console.WriteLine("2. List new item          (later)");
                Console.WriteLine("3. Browse items           (later)");
                Console.WriteLine("4. Logout");
                Console.WriteLine("0. Exit");
            }
            
            Console.Write("\nChoice: ");
            var choice = Console.ReadLine()?.Trim();
            
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
                            ; // later: list item
                        break;
                    case "4":
                        if (market.Auth.IsLoggedIn)
                            market.Auth.Logout();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        private static void PrintHeader(AuthService auth)
        {
            Console.WriteLine("=== Second-Hand Market ===");
            Console.WriteLine(auth.IsLoggedIn
                ? $"Logged in as: {auth.CurrentUsername}"
                : "Not logged in");
            Console.WriteLine(new string('-', 35));
        }
        
        private static void RegisterFlow(AuthService auth)
        {
            Console.Write("Choose username: ");
            var username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Password: ");
            var password = Console.ReadLine() ?? "";

            auth.Register(username, password);
        }
        
        private static void LoginFlow(AuthService auth)
        {
            Console.Write("Username: ");
            var username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Password: ");
            var password = Console.ReadLine() ?? "";

            if (!auth.Login(username, password))
                Console.WriteLine("Login failed – wrong username or password.");
        }
        
        private static void ShowProfile(User user)
        {
            Console.WriteLine($"\nProfile of {user.Username}");
            Console.WriteLine($"Member since: {user.RegisteredAt:yyyy-MM-dd}");
            Console.WriteLine($"Listings:     {user.Listings.Count}");
            Console.WriteLine($"Purchases:    {user.Purchases.Count}");
            Console.WriteLine($"Reviews:      {user.ReceivedReviews.Count}");

            if (user.ReceivedReviews.Any())
            {
                Console.WriteLine("\nRecent reviews:");
                foreach (var r in user.ReceivedReviews.Take(3))
                    Console.WriteLine($"  • {r.Rating}/5 – {r.Comment}");
            }
        }
    }
}