namespace Arbeidskrav2
{
    public class SecondHandMarket
    {
        private readonly List<User> _users = new();
        public AuthService Auth { get; }
        
        public SecondHandMarket()
        {
            Auth = new AuthService(_users);
        }
        
        public IReadOnlyList<Listing> ActiveListings 
            => _listings.Where(l => l.Status == ListingStatus.Available)
                .OrderByDescending(l => l.CreatedAt)
                .ToList()
                .AsReadOnly();

        private readonly List<Listing> _listings = new();

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
    }
}