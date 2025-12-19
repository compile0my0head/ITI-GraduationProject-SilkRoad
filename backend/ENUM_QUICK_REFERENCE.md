# ?? Enum Quick Reference Card

## ? All Enums Stored as Strings

| Enum | Values | Example API Call |
|------|--------|------------------|
| **OrderStatus** | Pending, Accepted, Shipped, Delivered, Rejected, Cancelled, Refunded | `GET /api/orders?status=Pending` |
| **CampaignStage** | Draft, InReview, Scheduled, Ready, Published | `GET /api/campaigns?stage=Draft` |
| **PublishStatus** | Pending, Publishing, Published, Failed | - |
| **PlatformName** | Facebook, Instagram, TikTok, YouTube | `GET /api/social-platforms/available-platforms` |
| **TeamRole** | Owner, Moderator, Member | `POST /api/teams/{id}/members` |
| **MessageType** | Text, Image, Video, File | `POST /api/chatbot-faq` |

---

## ?? Common API Queries

### Orders
```bash
# Get all pending orders
GET /api/orders?status=Pending

# Get accepted orders
GET /api/orders?status=Accepted

# Accept an order
PUT /api/orders/{orderId}/accept

# Reject an order
PUT /api/orders/{orderId}/reject
```

### Campaigns
```bash
# Get draft campaigns
GET /api/campaigns?stage=Draft

# Get published campaigns
GET /api/campaigns?stage=Published
```

---

## ?? Frontend TypeScript Definitions

```typescript
enum OrderStatus {
  Pending = "Pending",
  Accepted = "Accepted",
  Shipped = "Shipped",
  Delivered = "Delivered",
  Rejected = "Rejected",
  Cancelled = "Cancelled",
  Refunded = "Refunded"
}

enum CampaignStage {
  Draft = "Draft",
  InReview = "InReview",
  Scheduled = "Scheduled",
  Ready = "Ready",
  Published = "Published"
}

enum PublishStatus {
  Pending = "Pending",
  Publishing = "Publishing",
  Published = "Published",
  Failed = "Failed"
}

enum PlatformName {
  Facebook = "Facebook",
  Instagram = "Instagram",
  TikTok = "TikTok",
  YouTube = "YouTube"
}

enum TeamRole {
  Owner = "Owner",
  Moderator = "Moderator",
  Member = "Member"
}

enum MessageType {
  Text = "Text",
  Image = "Image",
  Video = "Video",
  File = "File"
}
```

---

## ?? Backend Configuration Pattern

```csharp
// Entity Configuration
public class YourEntityConfiguration : IEntityTypeConfiguration<YourEntity>
{
    public void Configure(EntityTypeBuilder<YourEntity> builder)
    {
        builder.Property(e => e.YourEnumProperty)
            .IsRequired()
            .HasConversion<string>()  // ? Store as string
            .HasMaxLength(50);        // ? Set appropriate length
        
        builder.HasIndex(e => e.YourEnumProperty);  // ? Add index
    }
}
```

---

## ?? Key Points

- ? **No integers** - All enums stored as strings
- ? **No lookup tables** - Enums live in code
- ? **Human readable** - Database values are clear
- ? **API friendly** - String values in requests/responses
- ? **Indexed** - Fast query performance
- ? **Type safe** - EF Core handles conversion

---

## ?? Important Notes

### DO ?
- Use string values in API calls: `status=Pending`
- Use TypeScript string enums in frontend
- Query with enum names: `WHERE Status = 'Pending'`

### DON'T ?
- Use integer values: `status=0` ?
- Use numeric enums in TypeScript: `OrderStatus.Pending = 0` ?
- Create lookup tables for enums ?

---

## ?? Example Response

```json
{
  "id": "guid",
  "orderId": "ORD-12345",
  "customerId": "guid",
  "customerName": "John Doe",
  "totalPrice": 99.99,
  "status": "Pending",
  "statusDisplayName": "Pending",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

---

## ?? Quick Start

1. **Frontend**: Use string enum values
2. **API Calls**: Pass enum names as strings
3. **Backend**: EF Core converts automatically
4. **Database**: Stores readable string values

---

**Need Help?**
- See `ENUM_STRING_STORAGE_COMPLETE.md` for full details
- See `FRONTEND_INTEGRATION_GUIDE.md` for API reference
- See `ENUM_STRING_STORAGE_SUMMARY.md` for technical info
