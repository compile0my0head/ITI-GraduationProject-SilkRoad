using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Product Entity - Represents a product in the system
/// This is in the DOMAIN layer - contains only business logic, no dependencies
/// </summary>
public class Product : BaseEntity
{
    public string? ExternalProductID { get; set; } // Meta ID
    public int StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public string? Condition { get; set; }
    public string? Url { get; set; }
    public string? RetailerId { get; set; }
    
    // Navigation properties
    public Store? Store { get; set; }
    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    // Business logic methods
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative");
        
        Price = newPrice;
    }

    public void MarkAsOutOfStock()
    {
        InStock = false;
    }

    public void MarkAsInStock()
    {
        InStock = true;
    }
}

