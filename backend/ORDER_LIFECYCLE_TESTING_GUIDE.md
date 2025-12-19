# Order Lifecycle Testing Guide

## Quick Test with cURL/Postman

### Prerequisites
- Backend running on `http://localhost:5000`
- Valid JWT token
- Valid Store ID
- At least one Customer exists

---

## Test Scenario 1: Create Pending Order

### Step 1: Create an Order
```bash
POST http://localhost:5000/api/orders
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
Content-Type: application/json

{
  "customerId": "CUSTOMER_GUID",
  "totalPrice": 99.99
}
```

**Expected Response (201 Created)**:
```json
{
  "id": "NEW_ORDER_GUID",
  "storeId": 1,
  "customerId": "CUSTOMER_GUID",
  "customerName": "John Doe",
  "totalPrice": 99.99,
  "status": "Pending",
  "statusDisplayName": "Pending",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

? **Verify**: Status is `"Pending"` (string value)

---

## Test Scenario 2: Get Pending Orders

### Step 2: Filter by Pending Status
```bash
GET http://localhost:5000/api/orders?status=0
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

**Expected Response (200 OK)**:
```json
[
  {
    "id": "ORDER_GUID",
    "status": "Pending",
    "statusDisplayName": "Pending",
    "customerName": "John Doe",
    "totalPrice": 99.99,
    ...
  }
]
```

? **Verify**: Only orders with Status = "Pending" are returned

---

## Test Scenario 3: Accept Order

### Step 3: Accept the Pending Order
```bash
PUT http://localhost:5000/api/orders/{ORDER_GUID}/accept
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

**Expected Response (200 OK)**:
```json
{
  "id": "ORDER_GUID",
  "storeId": 1,
  "customerId": "CUSTOMER_GUID",
  "customerName": "John Doe",
  "totalPrice": 99.99,
  "status": "Accepted",
  "statusDisplayName": "Accepted",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

? **Verify**: Status changed from `"Pending"` to `"Accepted"`

---

## Test Scenario 4: Try to Accept Again (Should Fail)

### Step 4: Attempt to Accept Already Accepted Order
```bash
PUT http://localhost:5000/api/orders/{ORDER_GUID}/accept
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

**Expected Response (400 Bad Request)**:
```json
{
  "message": "Cannot accept order. Order status is 'Accepted'. Only orders with 'Pending' status can be accepted."
}
```

? **Verify**: Proper validation message returned

---

## Test Scenario 5: Reject Order

### Step 5: Create Another Order and Reject It
```bash
# Create order
POST http://localhost:5000/api/orders
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
Content-Type: application/json

{
  "customerId": "CUSTOMER_GUID",
  "totalPrice": 49.99
}

# Then reject it
PUT http://localhost:5000/api/orders/{NEW_ORDER_GUID}/reject
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

**Expected Response (200 OK)**:
```json
{
  "id": "NEW_ORDER_GUID",
  "status": "Rejected",
  "statusDisplayName": "Rejected",
  ...
}
```

? **Verify**: Status is `"Rejected"` (string value)

---

## Test Scenario 6: Verify Order Still Exists

### Step 6: Get Rejected Order by ID
```bash
GET http://localhost:5000/api/orders/{NEW_ORDER_GUID}
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

**Expected Response (200 OK)**:
```json
{
  "id": "NEW_ORDER_GUID",
  "status": 4,
  "statusDisplayName": "Rejected",
  ...
}
```

? **Verify**: Order still exists (not deleted)

---

## Test Scenario 7: Filter by Multiple Statuses

### Step 7a: Get All Orders
```bash
GET http://localhost:5000/api/orders
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

? **Verify**: Returns orders with all statuses

### Step 7b: Get Accepted Orders
```bash
GET http://localhost:5000/api/orders?status=Accepted
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

? **Verify**: Only Status = "Accepted" orders returned

### Step 7c: Get Rejected Orders
```bash
GET http://localhost:5000/api/orders?status=Rejected
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

? **Verify**: Only Status = "Rejected" orders returned

---

## Test Scenario 8: Invalid Order ID

### Step 8: Try to Accept Non-Existent Order
```bash
PUT http://localhost:5000/api/orders/00000000-0000-0000-0000-000000000000/accept
Authorization: Bearer YOUR_TOKEN
X-Store-ID: YOUR_STORE_ID
```

**Expected Response (404 Not Found)**:
```json
{
  "message": "Order with ID 00000000-0000-0000-0000-000000000000 not found."
}
```

? **Verify**: Proper 404 error

---

## Test Scenario 9: Store Isolation

### Step 9: Try to Accept Order from Different Store
```bash
PUT http://localhost:5000/api/orders/{ORDER_GUID}/accept
Authorization: Bearer YOUR_TOKEN
X-Store-ID: DIFFERENT_STORE_ID
```

**Expected Response (404 Not Found)**:
```json
{
  "message": "Order with ID {ORDER_GUID} not found."
}
```

? **Verify**: EF Core query filters block cross-store access

---

## Postman Collection

### Import this JSON into Postman:

```json
{
  "info": {
    "name": "Order Lifecycle Tests",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000",
      "type": "string"
    },
    {
      "key": "token",
      "value": "YOUR_JWT_TOKEN",
      "type": "string"
    },
    {
      "key": "storeId",
      "value": "YOUR_STORE_ID",
      "type": "string"
    }
  ],
  "item": [
    {
      "name": "Create Pending Order",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          },
          {
            "key": "X-Store-ID",
            "value": "{{storeId}}"
          },
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"customerId\": \"CUSTOMER_GUID\",\n  \"totalPrice\": 99.99\n}"
        },
        "url": {
          "raw": "{{baseUrl}}/api/orders",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders"]
        }
      }
    },
    {
      "name": "Get Pending Orders",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          },
          {
            "key": "X-Store-ID",
            "value": "{{storeId}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/orders?status=0",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders"],
          "query": [
            {
              "key": "status",
              "value": "0"
            }
          ]
        }
      }
    },
    {
      "name": "Accept Order",
      "request": {
        "method": "PUT",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          },
          {
            "key": "X-Store-ID",
            "value": "{{storeId}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/orders/{{orderId}}/accept",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders", "{{orderId}}", "accept"]
        }
      }
    },
    {
      "name": "Reject Order",
      "request": {
        "method": "PUT",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          },
          {
            "key": "X-Store-ID",
            "value": "{{storeId}}"
          }
        ],
        "url": {
          "raw": "{{baseUrl}}/api/orders/{{orderId}}/reject",
          "host": ["{{baseUrl}}"],
          "path": ["api", "orders", "{{orderId}}", "reject"]
        }
      }
    }
  ]
}
```

---

## Database Verification

### Check Status Column Exists
```sql
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    IS_NULLABLE, 
    COLUMN_DEFAULT
FROM 
    INFORMATION_SCHEMA.COLUMNS
WHERE 
    TABLE_NAME = 'Orders' 
    AND COLUMN_NAME = 'Status';
```

**Expected Result**:
| COLUMN_NAME | DATA_TYPE | IS_NULLABLE | COLUMN_DEFAULT |
|-------------|-----------|-------------|----------------|
| Status      | int       | NO          | ((0))          |

---

### Check Existing Orders Have Status
```sql
SELECT 
    Id, 
    CustomerId, 
    TotalPrice, 
    Status,
    CASE Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Accepted'
        WHEN 2 THEN 'Shipped'
        WHEN 3 THEN 'Delivered'
        WHEN 4 THEN 'Rejected'
        WHEN 5 THEN 'Cancelled'
        WHEN 6 THEN 'Refunded'
    END AS StatusName
FROM 
    Orders
ORDER BY 
    CreatedAt DESC;
```

---

## Expected Test Results Summary

| Test | Expected Outcome | Status |
|------|------------------|--------|
| Create Order | Status = Pending (0) | ? |
| Filter Pending | Only Pending orders | ? |
| Accept Order | Status ? Accepted (1) | ? |
| Accept Again | 400 Error | ? |
| Reject Order | Status ? Rejected (4) | ? |
| Order Exists | Still in DB | ? |
| Filter Accepted | Only Accepted orders | ? |
| Invalid ID | 404 Error | ? |
| Cross-Store | 404 Error | ? |

---

## Common Issues & Solutions

### Issue: 401 Unauthorized
**Solution**: Check that JWT token is valid and not expired

### Issue: 403 Forbidden
**Solution**: Ensure X-Store-ID header is included

### Issue: 404 Not Found for Valid Order
**Solution**: Verify order belongs to the store specified in X-Store-ID header

### Issue: Cannot accept order (400)
**Solution**: Check order status - only Pending orders can be accepted

---

**End of Testing Guide**
