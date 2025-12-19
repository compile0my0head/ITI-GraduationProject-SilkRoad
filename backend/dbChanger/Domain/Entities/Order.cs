using Domain.Common;

namespace Domain.Entities;

public class Order : BaseEntity
{
    public int StoreId { get; set; }
    public int CustomerID { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Navigation properties
    public Store? Store { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}

