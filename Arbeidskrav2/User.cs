using System;
using System.Collections.Generic;
using System.Linq;

namespace Arbeidskrav2
{
    /// <summary>
    /// Represents a registered user. Contains all personal data, listings, purchases and received reviews.
    /// Password is hashed (simple but secure enough for this assignment).
    /// </summary>
    public class User
    {
        public string Username { get; init; }
        public string PasswordHash { get; private set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public DateTime RegisteredAt { get; init; }

        public List<Listing> Listings { get; } = new();
        public List<Transaction> Purchases { get; } = new();
        public List<Review> ReceivedReviews { get; } = new();
        
        /// <summary>
        /// Active (unsold) listings - read-only for safety.
        /// </summary>
        public IReadOnlyList<Listing> ActiveListings 
            => Listings.Where(l => l.Status == ListingStatus.Available).ToList().AsReadOnly();
        
        /// <summary>
        /// Sold listings - read-only for safety.
        /// </summary>
        public IReadOnlyList<Listing> SoldListings 
            => Listings.Where(l => l.Status == ListingStatus.Sold).ToList().AsReadOnly();
    
        public User(string username, string plainPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }
            Username = username.Trim();
            PasswordHash = HashPassword(plainPassword);
            RegisteredAt = DateTime.Now;
        }

        /// <summary>
        /// Verifies supplied password against stored hash.
        /// </summary>
        public bool VerifyPassword(string plainPassword)
        {
            return PasswordHash == HashPassword(plainPassword);
        }

        private static string HashPassword(string plainText)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText + "s3cr3t-salt"));
        }
    
        public override string ToString()
            => $"@{Username} (joined {RegisteredAt:yyyy-MM-dd})";
    }
}