using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Product : BaseEntity
{
    [MaxLength(50)]
    public string? ExternalProductID { get; set; }
    
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [MaxLength(5000)]
    public string ProductDescription { get; set; } = string.Empty;
    
    public decimal ProductPrice { get; set; }
    
    public bool InStock { get; set; }
    
    [MaxLength(50)]
    public string? Brand { get; set; }
    
    [MaxLength(500)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(50)]
    public string? Condition { get; set; }
    
    [MaxLength(500)]
    public string? Url { get; set; }
    
    [MaxLength(50)]
    public string? RetailerId { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ICollection<OrderProduct> OrderProducts { get; set; } = new HashSet<OrderProduct>();
    public ICollection<Campaign> Campaigns { get; set; } = new HashSet<Campaign>();


    // Business logic methods
    public void MarkAsOutOfStock()
    {
        InStock = false;
    }

    public void MarkAsInStock()
    {
        InStock = true;
    }
}
