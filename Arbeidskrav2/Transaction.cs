using System;

namespace Arbeidskrav2
{
    /// <summary>
    /// Represents a completed purchase transaction.
    /// Links listing, buyer, seller, price and timestamp.
    /// Used for both buyer and seller history.
    /// </summary>
    public class Transaction
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Listing Listing { get; init; }
        public User Buyer { get; init; }
        public User Seller { get; init; }
        public decimal Price { get; init; }
        public DateTime PurchasedAt { get; init; } = DateTime.Now;

        /// <summary>
        /// Creates a new transaction when an item is purchased.
        /// </summary>
        public Transaction(Listing listing, User buyer)
        {
            Listing = listing ?? throw new ArgumentNullException(nameof(listing));
            Buyer = buyer ?? throw new ArgumentNullException(nameof(buyer));
            Seller = listing.Seller;
            Price = listing.Price;
        }
        
        public override string ToString()
            => $"{Listing.Title} - {Price:N0} NOK - Bought by @{Buyer.Username} on {PurchasedAt:yyyy-MM-dd}";
    }
}