using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Customer : BaseEntity
{
    [Required]
    public Guid StoreId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? BillingAddress { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? PSID { get; set; }
    
    // Navigation properties
    public Store Store { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
}

