using System;

namespace Arbeidskrav2
{
    /// <summary>
    /// Core entity for an item listing. Encapsulates all listing data and behaviour.
    /// Only the seller can edit/delete. Uses enums for category/condition/status.
    /// </summary>
    public class Listing
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        
        public string Title { get; private set; }
        public string Description { get; private set; }
        public Category Category { get; private set; }
        public Condition Condition { get; private set; }
        public decimal Price { get; private set; }
        public ListingStatus Status { get; private set; } = ListingStatus.Available;
        
        public User Seller { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.Now;
        public DateTime? SoldAt { get; private set; }

        /// <summary>
        /// Display-friendly category name (matches assignment spec exactly, including "&").
        /// </summary>
        public string CategoryDisplay =>
            Category switch
            {
                Category.ClothingAndAccessories => "Clothing & Accessories",
                Category.FurnitureAndHome => "Furniture & Home",
                Category.BooksAndMedia => "Books & Media",
                Category.SportsAndOutdoors => "Sports & Outdoors",
                _ => Category.ToString()
            };

        /// <summary>
        /// Display-friendly condition name (matches assignment spec and sample output).
        /// </summary>
        public string ConditionDisplay =>
            Condition switch
            {
                Condition.LikeNew => "Like New",
                _ => Condition.ToString()
            };

        public Listing(
            User seller,
            string title,
            string description,
            Category category,
            Condition condition,
            decimal price)
        {
            if (seller == null) throw new ArgumentNullException(nameof(seller));
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
            if (price < 0) throw new ArgumentException("Price can not be negative.");
            
            Seller = seller;
            Title = title.Trim();
            Description = (description ?? "").Trim();
            Category = category;
            Condition = condition;
            Price = price;
        }

        /// <summary>
        /// Allows seller to update title, description or price (only while Available).
        /// </summary>
        public void UpdateDetails(
            string? newTitle = null,
            string? newDescription = null,
            decimal? newPrice = null)
        {
            if (Status != ListingStatus.Available)
                throw new InvalidOperationException("Cannot edit a sold listing.");

            if (newTitle != null && !string.IsNullOrWhiteSpace(newTitle))
                Title = newTitle.Trim();

            if (newDescription != null)
                Description = newDescription.Trim();
            
            if (newPrice.HasValue)
            {
                if (newPrice.Value < 0)
                    throw new ArgumentException("Price cannot be negative.");
                Price = newPrice.Value;
            }
        }
        
        internal void MarkAsSold()
        {
            if (Status == ListingStatus.Sold)
                return;
            
            Status = ListingStatus.Sold;
            SoldAt = DateTime.Now;
        }
        
        public bool IsOwnedBy(User user) => Seller == user;
        
        public override string ToString()
        {
            return $"{Title} ({CategoryDisplay}) – {ConditionDisplay} – {Price:N0} NOK – {Status}";
        }
        
        public string ShortInfo()
        {
            return $"{Title,-35} {Price,8:N0} NOK  {ConditionDisplay,-8} {CategoryDisplay}";
        }
    }
}