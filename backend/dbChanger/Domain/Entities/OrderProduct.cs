namespace Domain.Entities;

/// <summary>
/// Junction table for Many-to-Many relationship between Order and Product
/// </summary>
public class OrderProduct
{
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    
    // Soft delete properties
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }
    
    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}

