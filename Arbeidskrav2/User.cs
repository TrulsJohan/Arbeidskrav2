using System.Transactions;

namespace Arbeidskrav2
{
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
    
        public User (string username, string plainPassword)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }
            Username = username.Trim();
            PasswordHash = HashPassword(plainPassword);
            RegisteredAt = DateTime.Now;
        }

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