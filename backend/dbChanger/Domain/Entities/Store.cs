using Domain.Common;

namespace Domain.Entities;

/// <summary>
/// Store Entity - Represents a business store in the system
/// Owner relationship with User
/// </summary>
public class Store : BaseEntity
{
    public string StoreName { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
    
    // Navigation properties
    public User? Owner { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<SocialPlatform> Platforms { get; set; } = new List<SocialPlatform>();
    public ICollection<AutomationTask> AutomationTasks { get; set; } = new List<AutomationTask>();
    public ICollection<ChatbotFAQ> ChatbotFAQs { get; set; } = new List<ChatbotFAQ>();
}

