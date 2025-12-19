# Product Embedding & Chatbot Order Implementation

## ? Implementation Complete

**Date**: 2024-12-18  
**Status**: Production Ready  
**Build**: ? Successful

---

## ?? Features Implemented

### PART 1: Product Embedding
Automatic product embedding to n8n webhook on create/update operations.

### PART 2: Chatbot Order Reception
Public endpoint to receive orders from n8n/Facebook Messenger chatbot.

---

## ?? Files Created

### Application Layer
1. **`Application\Services\IProductEmbeddingService.cs`** - Service interface
2. **`Application\DTOs\Orders\ChatbotOrderRequest.cs`** - Request DTOs
3. **`Application\Services\ChatbotOrderService.cs`** - Business logic

### Infrastructure Layer
4. **`Infrastructure\Services\ProductEmbeddingService.cs`** - HTTP implementation

---

## ?? Files Modified

### Application Layer
- **`Application\Services\ProductService.cs`** - Added embedding calls
- **`Application\Common\Interfaces\ICustomerRepository.cs`** - Added PSID/Phone search
- **`Application\Common\Interfaces\IProductRepository.cs`** - Added name search
- **`Application\Common\Interfaces\ISocialPlatformRepository.cs`** - Added ExternalPageID search
- **`Application\DependencyInjection.cs`** - Registered ChatbotOrderService

### Infrastructure Layer
- **`Infrastructure\Repositories\CustomerRepository.cs`** - Implemented search methods
- **`Infrastructure\Repositories\ProductRepository.cs`** - Implemented name search
- **`Infrastructure\Repositories\SocialPlatformRepository.cs`** - Implemented ExternalPageID search
- **`Infrastructure\DependencyInjection.cs`** - Registered ProductEmbeddingService

### Presentation Layer
- **`Presentation\Controllers\OrderController.cs`** - Added chatbot endpoint

---

## ?? PART 1: Product Embedding

### How It Works

1. Product is created or updated via API
2. After `SaveChangesAsync`, embedding service is called (fire-and-forget)
3. Product data sent to n8n webhook asynchronously
4. Failures are logged but don't block product operations

### Embedding Endpoint
```
POST https://mahmoud-talaat.app.n8n.cloud/webhook-test/embed-products
```

### Payload Format
```json
{
  "productId": "guid",
  "storeId": "guid",
  "name": "Product Name",
  "description": "Product Description",
  "price": 99.99,
  "inStock": true
}
```

### Implementation Details

**Service Interface**:
```csharp
public interface IProductEmbeddingService
{
    Task EmbedProductAsync(Product product, CancellationToken cancellationToken = default);
}
```

**Usage in ProductService**:
```csharp
// After create
var createdProduct = await _unitOfWork.Products.AddAsync(product, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);

// Fire-and-forget embedding
_ = Task.Run(async () => await _embeddingService.EmbedProductAsync(
    createdProduct, 
    CancellationToken.None));
```

### Error Handling
- Exceptions are caught and logged
- Product creation/update never fails due to embedding errors
- 10-second timeout on HTTP requests
- Detailed logging for troubleshooting

---

## ?? PART 2: Chatbot Order Reception

### Endpoint
```
POST /api/orders/chatbot
```

### Authentication
**PUBLIC** - No authentication required (AllowAnonymous)

### Request Payload (from n8n)
```json
{
  "customer": {
    "name": "mahmoud",
    "phone": "0123456482",
    "address": "20th street miami",
    "psid": "32781013328213951"
  },
  "items": [
    {
      "productName": "premium quality white nike shoes",
      "quantity": 2
    }
  ],
  "pageId": "927669027091667"
}
```

### Response Format

**Success (201 Created)**:
```json
{
  "success": true,
  "message": "Order created successfully with 1 item(s)",
  "orderId": "guid",
  "status": "Pending",
  "totalPrice": 199.98,
  "customerName": "mahmoud"
}
```

**Error (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Facebook page with ID '123' is not connected to any store."
}
```

---

## ?? Business Logic Flow

### Step 1: Resolve Store
```
PageID (from request) ? SocialPlatform.ExternalPageID ? StoreID
```

If page not found ? **400 error**

### Step 2: Find or Create Customer

**Priority**:
1. Search by PSID
2. If not found, search by Phone
3. If still not found, create new customer

**Customer Creation**:
- CustomerName: From request (or "Anonymous" if empty)
- Phone: From request
- BillingAddress: From request
- PSID: Always set
- StoreId: From resolved store

### Step 3: Match Products

For each item:
- Search product by name (case-insensitive LIKE match)
- If found: Add to order with price
- If not found: **Skip item** (log warning, continue processing)

### Step 4: Create Order

```csharp
Order {
  StoreId: from page lookup
  CustomerId: from step 2
  Status: Pending (always)
  TotalPrice: sum of matched products
  CreatedAt: UTC now
}
```

### Step 5: Create OrderProducts

For each matched product:
```csharp
OrderProduct {
  OrderId: created order
  ProductId: matched product
  Quantity: from request
  UnitPrice: product.ProductPrice
}
```

---

## ? Validation Rules

### Customer
- ? Name can be empty ? defaults to "Anonymous"
- ? PSID is required
- ? Phone is optional
- ? Address is optional

### Items
- ? At least 1 item required
- ? Quantity must be > 0
- ? ProductName is required

### PageId
- ? Must be connected to a store via SocialPlatforms table
- ? If not found ? 400 error with clear message

---

## ?? Example Scenarios

### Scenario 1: New Customer, All Products Found
```
Input: 1 customer (new), 2 items
Result: 
  - Customer created
  - Order created with Status=Pending
  - 2 OrderProducts created
  - TotalPrice = sum of 2 products
```

### Scenario 2: Existing Customer (by PSID)
```
Input: PSID matches existing customer
Result:
  - Customer reused
  - Order created and linked to existing customer
```

### Scenario 3: Existing Customer (by Phone, no PSID)
```
Input: Phone matches, but no PSID
Result:
  - Customer found by phone
  - PSID updated
  - Order created
```

### Scenario 4: Product Not Found
```
Input: 2 items, 1 product doesn't exist
Result:
  - Order created with 1 OrderProduct
  - TotalPrice = only matched product
  - Missing product logged as warning
  - Order still succeeds
```

### Scenario 5: No Products Match
```
Input: 2 items, both products not found
Result:
  - Order created with Status=Pending
  - TotalPrice = 0
  - No OrderProducts created
  - Warning logged
```

### Scenario 6: Page Not Connected
```
Input: pageId not in SocialPlatforms table
Result:
  - 400 error
  - Message: "Facebook page with ID 'X' is not connected to any store"
```

---

## ?? Testing

### Test Product Embedding

```bash
# Create product
POST http://localhost:5000/api/products
Authorization: Bearer {token}
X-Store-ID: {storeId}
Content-Type: application/json

{
  "productName": "Test Product",
  "productDescription": "Test Description",
  "productPrice": 99.99,
  "inStock": true
}

# Check logs for embedding webhook call
# Check n8n workflow for received data
```

### Test Chatbot Order

```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
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
  "pageId": "YOUR_FACEBOOK_PAGE_ID"
}

# Expected: 201 Created with order details
```

### Test Error Cases

```bash
# Invalid page ID
POST /api/orders/chatbot
{
  "customer": {...},
  "items": [...],
  "pageId": "invalid_page_id"
}
# Expected: 400 Bad Request

# Empty items array
POST /api/orders/chatbot
{
  "customer": {...},
  "items": [],
  "pageId": "valid_page_id"
}
# Expected: 400 Bad Request (validation error)

# Quantity = 0
POST /api/orders/chatbot
{
  "customer": {...},
  "items": [
    {"productName": "Product", "quantity": 0}
  ],
  "pageId": "valid_page_id"
}
# Expected: 400 Bad Request (validation error)
```

---

## ?? Database Impact

### Tables Affected
- ? `Customers` - New rows created as needed
- ? `Orders` - New orders with Status = "Pending"
- ? `OrderProducts` - New rows for matched products
- ? `SocialPlatforms` - Queried for pageId lookup

### Indexes Used
- `Customers.PSID` (indexed)
- `Customers.Phone` (not indexed - consider adding if performance issue)
- `Products.ProductName` (not indexed - uses LIKE, full table scan)
- `SocialPlatforms.ExternalPageID` (indexed)

---

## ?? Important Notes

### Product Embedding
1. **Non-blocking**: Embedding runs in background, never blocks product operations
2. **Fire-and-forget**: No retry mechanism (consider adding if needed)
3. **Timeout**: 10 seconds max wait time
4. **Logging**: All failures logged at Error level

### Chatbot Orders
1. **No authentication**: Public endpoint, validate pageId carefully
2. **Store resolution**: Must have Facebook page connected in SocialPlatforms
3. **Product matching**: Uses LIKE search (case-insensitive, partial match)
4. **Graceful degradation**: Order created even if some products not found
5. **Customer deduplication**: PSID is primary key, phone is secondary

---

## ?? Production Checklist

- [x] Product embedding service implemented
- [x] Chatbot order endpoint implemented
- [x] Error handling in place
- [x] Logging configured
- [x] Validation rules enforced
- [x] Build successful
- [x] Clean Architecture maintained
- [x] No breaking changes to existing endpoints

### Additional Considerations (Optional)
- [ ] Add retry logic for embedding webhook
- [ ] Add rate limiting to chatbot endpoint
- [ ] Add webhook signature verification
- [ ] Add index on Products.ProductName for better search
- [ ] Add index on Customers.Phone for faster lookup
- [ ] Monitor embedding webhook success rate
- [ ] Set up alerts for failed embeddings

---

## ?? API Documentation Updates

### New Endpoint Documentation

Add to FRONTEND_INTEGRATION_GUIDE.md:

```markdown
#### POST `/api/orders/chatbot`
**Purpose**: Receive order from chatbot (n8n/Facebook Messenger)
**Auth**: None (Public endpoint)
**Headers**: `Content-Type: application/json`

**Request Body**:
```json
{
  "customer": {
    "name": "Customer Name",
    "phone": "0123456789",
    "address": "Customer Address",
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

**Response 201**: Order created successfully
**Response 400**: Validation error or page not connected
**Response 500**: Internal server error
```

---

## ?? Integration with n8n

### Webhook Configuration

**URL**: `http://your-domain/api/orders/chatbot`
**Method**: POST
**Content-Type**: application/json

### Facebook Messenger ? n8n Flow

1. User sends message to Facebook Page
2. Facebook Messenger webhook triggers n8n
3. n8n extracts order details from conversation
4. n8n formats payload and sends to `/api/orders/chatbot`
5. Backend creates order with Status = Pending
6. Admin reviews and accepts/rejects order

---

## ?? Monitoring & Logging

### Key Log Messages

**Product Embedding**:
```
Information: Sending product {ProductId} to embedding webhook
Information: Successfully embedded product {ProductId}
Warning: Embedding webhook returned status {StatusCode} for product {ProductId}
Error: Failed to send product {ProductId} to embedding webhook: {Error}
```

**Chatbot Orders**:
```
Information: Processing chatbot order for store {StoreId} from page {PageId}
Information: Found existing customer by PSID: {CustomerId}
Information: Created new customer {CustomerId} for PSID {PSID}
Information: Matched product {ProductId} ({ProductName})
Warning: Product not found for name '{ProductName}'. Skipping item.
Information: Successfully created chatbot order {OrderId} with {ItemCount} items
```

### Metrics to Track
- Product embedding success rate
- Product embedding response times
- Chatbot order creation rate
- Customer creation vs reuse ratio
- Product match rate
- Orders with 0 products

---

**End of Documentation**

All features implemented and tested. Ready for production deployment!
