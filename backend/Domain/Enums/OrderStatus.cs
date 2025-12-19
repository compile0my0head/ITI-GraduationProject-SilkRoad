namespace Domain.Enums;

/// <summary>
/// Order lifecycle status
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order created, waiting for admin approval
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Order accepted by admin
    /// </summary>
    Accepted = 1,
    
    /// <summary>
    /// Order shipped to customer
    /// </summary>
    Shipped = 2,
    
    /// <summary>
    /// Order delivered to customer
    /// </summary>
    Delivered = 3,
    
    /// <summary>
    /// Order rejected by admin
    /// </summary>
    Rejected = 4,
    
    /// <summary>
    /// Order cancelled (by customer or admin)
    /// </summary>
    Cancelled = 5,
    
    /// <summary>
    /// Order refunded
    /// </summary>
    Refunded = 6
}
