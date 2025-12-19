using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Store : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string StoreName { get; set; } = string.Empty;

    [Required]
    public Guid OwnerUserId { get; set; }

    // Navigation properties
    public User Owner { get; set; } = null!;
    public ICollection<Team> Teams { get; set; } = new HashSet<Team>();
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    public ICollection<Campaign> Campaigns { get; set; } = new HashSet<Campaign>();
    public ICollection<Customer> Customers { get; set; } = new HashSet<Customer>();
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<SocialPlatform> Platforms { get; set; } = new HashSet<SocialPlatform>();
    public ICollection<ChatbotFAQ> FAQs { get; set; } = new HashSet<ChatbotFAQ>();
    public ICollection<AutomationTask> AutomationTasks { get; set; } = new HashSet<AutomationTask>();
}

