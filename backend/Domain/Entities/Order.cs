using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Order : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    public Guid CustomerId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Order status - lifecycle tracking
    /// Defaults to Pending when order is created
    /// </summary>
    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderProduct> OrderProducts { get; set; } = new HashSet<OrderProduct>();
}

