using Domain.Common;

namespace Domain.Entities;

public class Customer : BaseEntity
{
    public int StoreId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }
    public string? Phone { get; set; }
    
    // Navigation properties
    public Store? Store { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

