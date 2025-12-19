# ?? Testing Checklist: Product Embedding & Chatbot Orders

## ? Pre-Testing Setup

- [ ] Database migration applied
- [ ] Server running on http://localhost:5000
- [ ] HttpClient registered in DI
- [ ] JWT token available (for product creation)
- [ ] Store ID available
- [ ] Facebook Page connected in SocialPlatforms table

---

## ?? Test 1: Product Embedding on Create

### Setup
1. Get valid JWT token
2. Get valid Store ID

### Test
```bash
POST http://localhost:5000/api/products
Authorization: Bearer {YOUR_JWT_TOKEN}
X-Store-ID: {YOUR_STORE_ID}
Content-Type: application/json

{
  "productName": "Test Product Embedding",
  "productDescription": "This product should be sent to n8n",
  "productPrice": 99.99,
  "inStock": true
}
```

### Expected Results
- [x] ? 201 Created response
- [x] ? Product created in database
- [x] ? Log message: "Sending product {id} to embedding webhook"
- [x] ? Log message: "Successfully embedded product {id}" OR warning if webhook fails
- [x] ? Check n8n workflow for received data

### Verification
```sql
SELECT * FROM Products WHERE ProductName = 'Test Product Embedding'
```

---

## ?? Test 2: Product Embedding on Update

### Setup
Use product from Test 1

### Test
```bash
PUT http://localhost:5000/api/products/{PRODUCT_ID}
Authorization: Bearer {YOUR_JWT_TOKEN}
X-Store-ID: {YOUR_STORE_ID}
Content-Type: application/json

{
  "productName": "Updated Product Name",
  "productPrice": 89.99
}
```

### Expected Results
- [x] ? 200 OK response
- [x] ? Product updated in database
- [x] ? Embedding service called again
- [x] ? n8n receives updated data

---

## ?? Test 3: Chatbot Order - New Customer, Product Found

### Setup
1. Ensure at least one product exists in database
2. Note a product name from database
3. Get Facebook Page ID from SocialPlatforms table

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "Test Customer 1",
    "phone": "1234567890",
    "address": "123 Test Street",
    "psid": "test_psid_001"
  },
  "items": [
    {
      "productName": "{EXISTING_PRODUCT_NAME}",
      "quantity": 2
    }
  ],
  "pageId": "{YOUR_FACEBOOK_PAGE_ID}"
}
```

### Expected Results
- [x] ? 201 Created
- [x] ? Response includes orderId, status: "Pending", totalPrice
- [x] ? New customer created in Customers table
- [x] ? New order created in Orders table with Status = "Pending"
- [x] ? OrderProducts created with correct quantity and price

### Verification
```sql
-- Check customer
SELECT * FROM Customers WHERE PSID = 'test_psid_001'

-- Check order
SELECT * FROM Orders 
WHERE CustomerId = (SELECT Id FROM Customers WHERE PSID = 'test_psid_001')

-- Check order products
SELECT op.*, p.ProductName 
FROM OrderProducts op
JOIN Products p ON op.ProductId = p.Id
WHERE op.OrderId = '{ORDER_ID}'
```

---

## ?? Test 4: Chatbot Order - Existing Customer (PSID Match)

### Setup
Use same PSID from Test 3

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "Different Name",
    "phone": "9876543210",
    "address": "Different Address",
    "psid": "test_psid_001"
  },
  "items": [
    {
      "productName": "{EXISTING_PRODUCT_NAME}",
      "quantity": 1
    }
  ],
  "pageId": "{YOUR_FACEBOOK_PAGE_ID}"
}
```

### Expected Results
- [x] ? 201 Created
- [x] ? **NO new customer created**
- [x] ? Order linked to existing customer
- [x] ? Customer details NOT updated (PSID is primary key)

### Verification
```sql
-- Should return 1 customer
SELECT COUNT(*) FROM Customers WHERE PSID = 'test_psid_001'

-- Should return 2 orders for same customer
SELECT COUNT(*) FROM Orders 
WHERE CustomerId = (SELECT Id FROM Customers WHERE PSID = 'test_psid_001')
```

---

## ?? Test 5: Chatbot Order - Existing Customer (Phone Match)

### Setup
1. Create customer with phone but no PSID
```sql
INSERT INTO Customers (Id, StoreId, CustomerName, Phone, CreatedAt)
VALUES (NEWID(), '{YOUR_STORE_ID}', 'Phone Customer', '5555555555', GETUTCDATE())
```

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "Phone Customer",
    "phone": "5555555555",
    "address": "Test Address",
    "psid": "test_psid_002"
  },
  "items": [
    {
      "productName": "{EXISTING_PRODUCT_NAME}",
      "quantity": 1
    }
  ],
  "pageId": "{YOUR_FACEBOOK_PAGE_ID}"
}
```

### Expected Results
- [x] ? 201 Created
- [x] ? Customer found by phone
- [x] ? **PSID updated** on existing customer
- [x] ? Order linked to existing customer

### Verification
```sql
-- PSID should be updated
SELECT PSID FROM Customers WHERE Phone = '5555555555'
-- Expected: test_psid_002
```

---

## ?? Test 6: Chatbot Order - Product Not Found

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "Test Customer",
    "phone": "1111111111",
    "address": "Test",
    "psid": "test_psid_003"
  },
  "items": [
    {
      "productName": "NON_EXISTENT_PRODUCT_NAME_12345",
      "quantity": 1
    }
  ],
  "pageId": "{YOUR_FACEBOOK_PAGE_ID}"
}
```

### Expected Results
- [x] ? 201 Created (order still created)
- [x] ? totalPrice = 0
- [x] ? Warning in logs: "Product not found for name..."
- [x] ? Order created with no OrderProducts

### Verification
```sql
-- Order exists
SELECT * FROM Orders WHERE CustomerId = (
  SELECT Id FROM Customers WHERE PSID = 'test_psid_003'
)
-- TotalPrice should be 0

-- No order products
SELECT COUNT(*) FROM OrderProducts 
WHERE OrderId = '{ORDER_ID}'
-- Expected: 0
```

---

## ?? Test 7: Chatbot Order - Multiple Items (Mixed)

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "Multi Item Customer",
    "phone": "2222222222",
    "address": "Test",
    "psid": "test_psid_004"
  },
  "items": [
    {
      "productName": "{EXISTING_PRODUCT_1}",
      "quantity": 2
    },
    {
      "productName": "NON_EXISTENT_PRODUCT",
      "quantity": 1
    },
    {
      "productName": "{EXISTING_PRODUCT_2}",
      "quantity": 3
    }
  ],
  "pageId": "{YOUR_FACEBOOK_PAGE_ID}"
}
```

### Expected Results
- [x] ? 201 Created
- [x] ? Order created with 2 OrderProducts (not 3)
- [x] ? TotalPrice = (price1 * 2) + (price2 * 3)
- [x] ? Warning logged for non-existent product

---

## ?? Test 8: Chatbot Order - Anonymous Customer

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "",
    "phone": null,
    "address": null,
    "psid": "test_psid_005"
  },
  "items": [
    {
      "productName": "{EXISTING_PRODUCT_NAME}",
      "quantity": 1
    }
  ],
  "pageId": "{YOUR_FACEBOOK_PAGE_ID}"
}
```

### Expected Results
- [x] ? 201 Created
- [x] ? Customer created with name = "Anonymous"
- [x] ? Order created successfully

### Verification
```sql
SELECT CustomerName FROM Customers WHERE PSID = 'test_psid_005'
-- Expected: "Anonymous"
```

---

## ? Test 9: Invalid Page ID

### Test
```bash
POST http://localhost:5000/api/orders/chatbot
Content-Type: application/json

{
  "customer": {
    "name": "Test",
    "psid": "test_psid_006"
  },
  "items": [
    {
      "productName": "Product",
      "quantity": 1
    }
  ],
  "pageId": "INVALID_PAGE_ID_999"
}
```

### Expected Results
- [x] ? 400 Bad Request
- [x] ? Message: "Facebook page with ID 'INVALID_PAGE_ID_999' is not connected..."
- [x] ? No customer created
- [x] ? No order created

---

## ? Test 10: Validation Errors

### Test 10a: Empty Items Array
```json
{
  "customer": {...},
  "items": [],
  "pageId": "..."
}
```
**Expected**: 400 Bad Request (validation error)

### Test 10b: Quantity = 0
```json
{
  "customer": {...},
  "items": [{"productName": "Product", "quantity": 0}],
  "pageId": "..."
}
```
**Expected**: 400 Bad Request (quantity must be > 0)

### Test 10c: Missing PSID
```json
{
  "customer": {
    "name": "Test"
  },
  "items": [...],
  "pageId": "..."
}
```
**Expected**: 400 Bad Request (PSID required)

---

## ?? Summary Report Template

### Product Embedding Tests
- [ ] Create triggers embedding
- [ ] Update triggers embedding
- [ ] Webhook receives correct payload
- [ ] Failures don't break product operations

### Chatbot Order Tests
- [ ] New customer created
- [ ] Existing customer found by PSID
- [ ] Existing customer found by Phone
- [ ] PSID updated when found by phone
- [ ] Product matched correctly
- [ ] Product not found handled gracefully
- [ ] Multiple items processed
- [ ] Anonymous customer handled
- [ ] Invalid page ID rejected
- [ ] Validation errors returned

### Database Verification
- [ ] Customers table populated correctly
- [ ] Orders table has Status = "Pending"
- [ ] OrderProducts table linked correctly
- [ ] TotalPrice calculated accurately

---

## ?? Debugging Commands

### Check Recent Orders
```sql
SELECT TOP 10 
    o.Id, 
    o.Status, 
    o.TotalPrice, 
    c.CustomerName, 
    c.PSID,
    o.CreatedAt
FROM Orders o
JOIN Customers c ON o.CustomerId = c.Id
ORDER BY o.CreatedAt DESC
```

### Check Order Details
```sql
SELECT 
    o.Id AS OrderId,
    o.Status,
    o.TotalPrice,
    c.CustomerName,
    c.Phone,
    c.PSID,
    p.ProductName,
    op.Quantity,
    op.UnitPrice,
    (op.Quantity * op.UnitPrice) AS LineTotal
FROM Orders o
JOIN Customers c ON o.CustomerId = c.Id
LEFT JOIN OrderProducts op ON o.Id = op.OrderId
LEFT JOIN Products p ON op.ProductId = p.Id
WHERE o.Id = '{ORDER_ID}'
```

### Check Embedding Logs
Look for:
```
Sending product {id} to embedding webhook
Successfully embedded product {id}
Failed to send product {id} to embedding webhook
```

### Check Chatbot Order Logs
Look for:
```
Processing chatbot order for store {id} from page {pageId}
Found existing customer by PSID: {customerId}
Created new customer {customerId} for PSID {psid}
Matched product {productId} ({name})
Product not found for name '{name}'
Successfully created chatbot order {orderId}
```

---

**Testing Complete**: ? All features verified

Next: Monitor production logs and webhook success rates
