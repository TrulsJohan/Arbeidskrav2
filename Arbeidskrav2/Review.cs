using System;

namespace Arbeidskrav2
{
    /// <summary>
    /// Represents a review left by a buyer for a seller after a transaction.
    /// Contains terningkast (1-6) and optional comment. One review per transaction.
    /// </summary>
    public class Review
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public User Reviewer { get; init; }
        public User Seller { get; init; }
        public Transaction Transaction { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime ReviewedAt { get; init; } = DateTime.Now;

        /// <summary>
        /// Creates a new review. Validates rating 1-6.
        /// </summary>
        public Review(User reviewer, User seller, Transaction transaction, int rating, string? comment)
        {
            if (rating < 1 || rating > 6)
                throw new ArgumentException("Rating must be between 1 and 6.");

            Reviewer = reviewer;
            Seller = seller;
            Transaction = transaction;
            Rating = rating;
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        }

        public override string ToString()
            => $"{Rating}/6 - {Comment ?? "No comment"}";
    }
}