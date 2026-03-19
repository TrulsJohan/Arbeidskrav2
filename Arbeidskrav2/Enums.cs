namespace Arbeidskrav2
{
    /// <summary>
    /// Predefined item categories (matches assignment spec, with display-friendly names in Listing).
    /// </summary>
    public enum Category
    {
        Electronics,
        ClothingAndAccessories,
        FurnitureAndHome,
        BooksAndMedia,
        SportsAndOutdoors,
        Other,
    }

    /// <summary>
    /// Item condition levels (matches assignment spec).
    /// </summary>
    public enum Condition
    {
        New,
        LikeNew,    
        Good,       
        Fair,
    }

    /// <summary>
    /// Listing status - only Available items can be purchased.
    /// </summary>
    public enum ListingStatus
    {
        Available,
        Sold,
    }
}