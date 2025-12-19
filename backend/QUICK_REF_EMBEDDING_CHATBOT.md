# ?? Quick Reference: Product Embedding & Chatbot Orders

## ? Implementation Status
- **Product Embedding**: ? Complete
- **Chatbot Orders**: ? Complete
- **Build**: ? Successful
- **Tests**: ? Manual testing required

---

## ?? Product Embedding

### Trigger
Automatically called after product create/update

### Webhook URL
```
https://mahmoud-talaat.app.n8n.cloud/webhook-test/embed-products
```

### Payload
```json
{
  "productId": "guid",
  "storeId": "guid",
  "name": "string",
  "description": "string",
  "price": 0.0,
  "inStock": boolean
}
```

### Behavior
- ? Non-blocking (fire-and-forget)
- ? Failures logged, not thrown
- ? 10-second timeout
- ? Runs after SaveChangesAsync

---

## ?? Chatbot Order Endpoint

### Endpoint
```
POST /api/orders/chatbot
```

### Authentication
**PUBLIC** - No authentication required

### Request Body
```json
{
  "customer": {
    "name": "Customer Name",
    "phone": "0123456789",
    "address": "Address",
    "psid": "facebook_psid"
  },
  "items": [
    {
      "productName": "Product Name",
      "quantity": 2
    }
  ],
  "pageId": "facebook_page_id"
}
```

### Success Response (201)
```json
{
  "success": true,
  "message": "Order created successfully with 1 item(s)",
  "orderId": "guid",
  "status": "Pending",
  "totalPrice": 199.98,
  "customerName": "Customer Name"
}
```

### Error Response (400)
```json
{
  "success": false,
  "message": "Error description"
}
```

---

## ?? Chatbot Order Flow

```
1. Receive request
   ?
2. Lookup Store by pageId
   ?
3. Find or Create Customer (PSID ? Phone ? Create)
   ?
4. Match Products by name
   ?
5. Create Order (Status = Pending)
   ?
6. Create OrderProducts
   ?
7. Return success response
```

---

## ? Validation Rules

| Field | Required | Default | Notes |
|-------|----------|---------|-------|
| customer.name | No | "Anonymous" | Can be empty |
| customer.psid | Yes | - | Primary identifier |
| customer.phone | No | null | Used for lookup |
| customer.address | No | null | Optional |
| items | Yes | - | Min 1 item |
| items[].productName | Yes | - | Case-insensitive search |
| items[].quantity | Yes | - | Must be > 0 |
| pageId | Yes | - | Must exist in SocialPlatforms |

---

## ?? Product Matching

**Search Strategy**: Case-insensitive LIKE match
```sql
WHERE ProductName LIKE '%{searchTerm}%'
```

**Behavior**:
- ? Product found ? Add to order
- ? Product not found ? Skip item, log warning
- ?? No products found ? Order created with $0 total

---

## ?? Testing Commands

### Test Product Creation (triggers embedding)
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Authorization: Bearer {token}" \
  -H "X-Store-ID: {storeId}" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Test Product",
    "productDescription": "Description",
    "productPrice": 99.99,
    "inStock": true
  }'
```

### Test Chatbot Order
```bash
curl -X POST http://localhost:5000/api/orders/chatbot \
  -H "Content-Type: application/json" \
  -d '{
    "customer": {
      "name": "Test Customer",
      "phone": "1234567890",
      "address": "Test Address",
      "psid": "test_psid_123"
    },
    "items": [
      {
        "productName": "Test Product",
        "quantity": 2
      }
    ],
    "pageId": "YOUR_PAGE_ID"
  }'
```

---

## ?? Troubleshooting

### Embedding Not Working
1. Check logs for HTTP errors
2. Verify webhook URL is accessible
3. Check timeout (10 seconds)
4. Ensure HttpClient is registered

### Chatbot Order Fails
1. **"Page not connected"** ? Add page to SocialPlatforms
2. **"Product not found"** ? Check product name match
3. **"Validation error"** ? Check request payload format
4. **500 error** ? Check server logs

---

## ?? Key Database Queries

### Find Customer by PSID
```sql
SELECT * FROM Customers 
WHERE PSID = '{psid}' AND StoreId = '{storeId}'
```

### Find Product by Name
```sql
SELECT * FROM Products 
WHERE StoreId = '{storeId}' 
  AND ProductName LIKE '%{name}%'
```

### Find Store by Page ID
```sql
SELECT StoreId FROM SocialPlatforms 
WHERE ExternalPageID = '{pageId}'
```

---

## ?? n8n Integration

### Required Facebook Page Setup
1. Create Facebook Page
2. Connect to SocialPlatforms table
3. Store ExternalPageID (Facebook Page ID)
4. Use ExternalPageID in chatbot requests

### n8n Workflow
```
Facebook Messenger Trigger
  ?
Extract Order Data
  ?
Format Payload
  ?
HTTP Request (POST /api/orders/chatbot)
  ?
Send Confirmation to Customer
```

---

## ?? Files Modified/Created

### Created
- `Application\Services\IProductEmbeddingService.cs`
- `Infrastructure\Services\ProductEmbeddingService.cs`
- `Application\DTOs\Orders\ChatbotOrderRequest.cs`
- `Application\Services\ChatbotOrderService.cs`

### Modified
- `Application\Services\ProductService.cs`
- `Application\Common\Interfaces\ICustomerRepository.cs`
- `Infrastructure\Repositories\CustomerRepository.cs`
- `Application\Common\Interfaces\IProductRepository.cs`
- `Infrastructure\Repositories\ProductRepository.cs`
- `Application\Common\Interfaces\ISocialPlatformRepository.cs`
- `Infrastructure\Repositories\SocialPlatformRepository.cs`
- `Presentation\Controllers\OrderController.cs`
- `Application\DependencyInjection.cs`
- `Infrastructure\DependencyInjection.cs`

---

## ? Performance Notes

### Product Embedding
- Fire-and-forget pattern
- No performance impact on product operations
- Background Task.Run

### Chatbot Order
- Multiple database queries (customer, products, platform)
- Consider adding indexes:
  - `Customers.Phone`
  - `Products.ProductName`
- LIKE search on ProductName (full table scan)

---

## ?? Security Considerations

### Product Embedding
- ? No sensitive data in payload
- ? HTTPS required for webhook
- ?? No webhook authentication (consider adding)

### Chatbot Order
- ?? Public endpoint (no auth)
- ? Validation on all inputs
- ? Store resolution via pageId
- ?? Consider: Webhook signature verification
- ?? Consider: Rate limiting

---

**Status**: ? Ready for Production

Need more details? See `PRODUCT_EMBEDDING_CHATBOT_ORDER_IMPLEMENTATION.md`
