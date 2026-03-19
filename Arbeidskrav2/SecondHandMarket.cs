using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbeidskrav2
{
    /// <summary>
    /// Main application core. Manages all listings, transactions, reviews and business rules.
    /// Uses LINQ extensively, generic collections, enums and exception handling.
    /// Fully separates data logic from UI (Program.cs).
    /// </summary>
    public class SecondHandMarket
    {
        private readonly List<User> _users = new();
        public AuthService Auth { get; }
        
        private readonly List<Listing> _listings = new();

        public SecondHandMarket()
        {
            Auth = new AuthService(_users);
        }
        
        /// <summary>
        /// All currently available listings (sorted newest first).
        /// </summary>
        public IReadOnlyList<Listing> ActiveListings 
            => _listings.Where(l => l.Status == ListingStatus.Available)
                .OrderByDescending(l => l.CreatedAt)
                .ToList()
                .AsReadOnly();

        /// <summary>
        /// Creates a new listing for the logged-in seller.
        /// </summary>
        public Listing CreateListing(
            User seller,
            string title,
            string description,
            Category category,
            Condition condition,
            decimal price)
        {
            if (seller == null) throw new ArgumentNullException(nameof(seller));
            if (!Auth.IsLoggedIn || Auth.CurrentUser != seller)
                throw new InvalidOperationException("Only the logged-in user can create listings.");

            var listing = new Listing(seller, title, description, category, condition, price);
    
            _listings.Add(listing);
            seller.Listings.Add(listing);

            Console.WriteLine($"Listing created: {listing.Title}");
            return listing;
        }

        /// <summary>
        /// Deletes a listing (only by owner and only if still available).
        /// </summary>
        public bool DeleteListing(Guid listingId)
        {
            var listing = _listings.FirstOrDefault(l => l.Id == listingId);
            if (listing == null) return false;

            if (!listing.IsOwnedBy(Auth.CurrentUser!))
            {
                Console.WriteLine("You can only delete your own listings.");
                return false;
            }

            if (listing.Status == ListingStatus.Sold)
            {
                Console.WriteLine("Cannot delete sold listings.");
                return false;
            }

            _listings.Remove(listing);
            Auth.CurrentUser!.Listings.Remove(listing);
            Console.WriteLine("Listing deleted.");
            return true;
        }

        public Listing? FindListing(Guid id) => _listings.FirstOrDefault(l => l.Id == id);
        
        /// <summary>
        /// Purchases a listing. Validates rules, marks sold, creates transaction.
        /// Returns the transaction for immediate review prompt.
        /// </summary>
        public Transaction? PurchaseListing(Guid listingId, User buyer)
        {
            var listing = _listings.FirstOrDefault(l => l.Id == listingId);

            if (listing == null) throw new InvalidOperationException("Listing not found.");
            if (listing.Status != ListingStatus.Available) throw new InvalidOperationException("Item is no longer available.");
            if (listing.IsOwnedBy(buyer)) throw new InvalidOperationException("You cannot buy your own listing.");

            var transaction = new Transaction(listing, buyer);
            listing.MarkAsSold();

            buyer.Purchases.Add(transaction);

            Console.WriteLine($"\nSuccess! You purchased '{listing.Title}' for {listing.Price:N0} NOK.");
            return transaction;
        }

        /// <summary>
        /// Returns filtered available listings using LINQ (category + keyword search).
        /// Fixed: correct keyword filtering logic.
        /// </summary>
        public List<Listing> GetFilteredListings(Category? categoryFilter = null, string? keyword = null)
        {
            var query = _listings.Where(l => l.Status == ListingStatus.Available);

            if (categoryFilter.HasValue)
            {
                query = query.Where(l => l.Category == categoryFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim().ToLower();
                query = query.Where(l =>
                    l.Title.ToLower().Contains(k) ||
                    l.Description.ToLower().Contains(k));
            }
            
            return query
                .OrderByDescending(l => l.CreatedAt)
                .ToList();
        }
        
        /// <summary>
        /// Leaves a review for a completed purchase (one per transaction).
        /// </summary>
        public Review? LeaveReview(User reviewer, Guid transactionId, int rating, string? comment)
        {
            var transaction = reviewer.Purchases.FirstOrDefault(t => t.Id == transactionId);

            if (transaction == null)
                throw new InvalidOperationException("Transaction not found in your purchase history.");

            if (transaction.Listing.Seller.ReceivedReviews.Any(r => r.Transaction.Id == transactionId))
                throw new InvalidOperationException("You have already reviewed this purchase.");

            var review = new Review(reviewer, transaction.Seller, transaction, rating, comment);

            transaction.Seller.ReceivedReviews.Add(review);
            Console.WriteLine($"\nThank you! You gave {rating}/6 to @{transaction.Seller.Username}.");
            return review;
        }
        
        /// <summary>
        /// Calculates average rating for a user using LINQ.
        /// </summary>
        public double? GetAverageRating(User user)
        {
            if (!user.ReceivedReviews.Any())
                return null;

            return Math.Round(user.ReceivedReviews.Average(r => r.Rating), 2);
        }
        
        /// <summary>
        /// Returns all transactions where this user was the seller.
        /// </summary>
        public List<Transaction> GetSoldTransactions(User seller)
        {
            return _listings
                .Where(l => l.Status == ListingStatus.Sold && l.Seller == seller)
                .Select(l => 
                {
                    var buyerTransaction = _users
                        .SelectMany(u => u.Purchases)
                        .FirstOrDefault(t => t.Listing.Id == l.Id);
                    return buyerTransaction;
                })
                .Where(t => t != null)
                .ToList()!;
        }
        
        /// <summary>
        /// Prints listings in a clear, readable format using display-friendly names.
        /// </summary>
        public void PrintListings(List<Listing> listings)
        {
            if (!listings.Any())
            {
                Console.WriteLine("No listings found.");
                return;
            }

            Console.WriteLine($"Found {listings.Count} listing(s):\n");

            foreach (var listing in listings)
            {
                Console.WriteLine($"ID: {listing.Id.ToString()[..8]}...");
                Console.WriteLine($"Title:       {listing.Title}");
                Console.WriteLine($"Price:       {listing.Price:N0} NOK");
                Console.WriteLine($"Category:    {listing.CategoryDisplay}");
                Console.WriteLine($"Condition:   {listing.ConditionDisplay}");
                Console.WriteLine($"Seller:      @{listing.Seller.Username}");
                Console.WriteLine($"Created:     {listing.CreatedAt:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"Description: {listing.Description}");
                Console.WriteLine(new string('-', 60));
            }
        }
        
        /// <summary>
        /// Prints a clean numbered table exactly like the assignment sample output.
        /// No more partial IDs — user just picks a number!
        /// </summary>
        public void PrintNumberedListings(List<Listing> listings)
        {
            Console.WriteLine("  #   Title                              Category               Condition   Price");
            Console.WriteLine("  ─────────────────────────────────────────────────────────────────────────────");

            for (int i = 0; i < listings.Count; i++)
            {
                var l = listings[i];
                Console.WriteLine($"  {i + 1,-3}{l.Title,-35} {l.CategoryDisplay,-22} {l.ConditionDisplay,-10} {l.Price,8:N0} kr");
            }
        }
    }
}