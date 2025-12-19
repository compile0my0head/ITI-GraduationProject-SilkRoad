# ??? Database Setup Guide

## Overview

**Data seeding has been removed** from the application due to Docker deployment issues. Instead, use one of the following methods to set up initial data.

---

## Method 1: Manual Data Entry via API (Recommended)

After deploying your application, use the Swagger UI or API endpoints to create initial data:

### Step 1: Register First User
```bash
POST /api/auth/register
{
  "fullName": "Admin User",
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

### Step 2: Login and Get JWT Token
```bash
POST /api/auth/login
{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```
**Response**: Copy the `token` value

### Step 3: Create First Store
```bash
POST /api/stores
Headers:
  Authorization: Bearer {your-token}

Body:
{
  "storeName": "My First Store",
  "storeDescription": "A sample store",
  "storeAddress": "123 Main Street",
  "phone": "+1234567890",
  "email": "store@example.com"
}
```

### Step 4: Add Products
```bash
POST /api/products
Headers:
  Authorization: Bearer {your-token}
  X-Store-ID: {store-id-from-step-3}

Body:
{
  "productName": "Sample Product",
  "productDescription": "Product description",
  "productPrice": 99.99,
  "inStock": true,
  "imageUrl": "https://via.placeholder.com/150"
}
```

---

## Method 2: SQL Scripts

Run these SQL scripts directly on your database after migrations:

### Create Sample Store
```sql
-- Insert sample user (requires hashed password from ASP.NET Identity)
-- Use the registration endpoint instead for proper password hashing

-- Insert sample store
INSERT INTO Stores (Id, StoreName, StoreDescription, StoreAddress, Phone, Email, IsActive, OwnerUserId, CreatedAt)
VALUES (
  NEWID(),
  'Nexus Tech Hub',
  'Your one-stop shop for the latest gadgets',
  '123 Silicon Avenue, Cairo, Egypt',
  '+201005550001',
  'contact@nexustech.com',
  1,
  'YOUR-USER-ID-HERE', -- Replace with actual user GUID
  GETUTCDATE()
);
```

### Create Sample Products
```sql
DECLARE @StoreId UNIQUEIDENTIFIER = 'YOUR-STORE-ID-HERE';

INSERT INTO Products (Id, ProductName, ProductDescription, ProductPrice, InStock, ImageUrl, StoreId, CreatedAt)
VALUES 
  (NEWID(), 'Laptop', 'High-performance laptop', 999.99, 1, 'https://via.placeholder.com/150', @StoreId, GETUTCDATE()),
  (NEWID(), 'Mouse', 'Wireless mouse', 29.99, 1, 'https://via.placeholder.com/150', @StoreId, GETUTCDATE()),
  (NEWID(), 'Keyboard', 'Mechanical keyboard', 79.99, 1, 'https://via.placeholder.com/150', @StoreId, GETUTCDATE());
```

---

## Method 3: EF Core Data Seeding (Development Only)

For local development, you can add data seeding in `SaasDbContext.OnModelCreating`:

### Add to SaasDbContext.cs
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ... existing configurations ...

    // ?? ONLY FOR DEVELOPMENT - Remove in production
    #if DEBUG
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
    {
        // Seed test data
        modelBuilder.Entity<Store>().HasData(
            new Store
            {
                Id = Guid.Parse("d3b07384-d9a1-4d3b-8215-08db3aa2c3f1"),
                StoreName = "Test Store",
                StoreDescription = "Development test store",
                StoreAddress = "123 Test Street",
                IsActive = true,
                OwnerUserId = Guid.Parse("57b57a02-28fb-48b9-6d1c-08de34eae319"), // Replace with valid user
                CreatedAt = DateTime.UtcNow
            }
        );
    }
    #endif
}
```

**Then run**:
```bash
dotnet ef migrations add SeedTestData --project Infrastructure --startup-project Presentation
dotnet ef database update --project Infrastructure --startup-project Presentation
```

---

## Method 4: Postman Collection

Import this Postman collection to quickly set up test data:

### Collection: Business Manager Setup
```json
{
  "info": {
    "name": "Business Manager - Initial Setup",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "1. Register User",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/auth/register",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"fullName\": \"Admin User\",\n  \"email\": \"admin@example.com\",\n  \"password\": \"Admin123!\"\n}"
        }
      }
    },
    {
      "name": "2. Login",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/auth/login",
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"admin@example.com\",\n  \"password\": \"Admin123!\"\n}"
        }
      }
    },
    {
      "name": "3. Create Store",
      "request": {
        "method": "POST",
        "url": "{{baseUrl}}/api/stores",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"storeName\": \"My Store\",\n  \"storeDescription\": \"Sample store\",\n  \"storeAddress\": \"123 Main St\"\n}"
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://your-app.onrender.com"
    },
    {
      "key": "token",
      "value": ""
    }
  ]
}
```

---

## Why Data Seeding Was Removed

1. **Docker Path Issues**: Relative file paths don't work in Docker containers
2. **Deployment Crashes**: Application crashes on startup when JSON files not found
3. **Testing-Only Feature**: Seeding is typically for development/testing, not production
4. **Security**: Production data should be created through APIs with proper validation
5. **Maintainability**: Easier to manage data through migrations or API calls

---

## Recommended Approach for Production

1. ? **Run migrations** to create schema
2. ? **Use API endpoints** to create initial data
3. ? **Set up admin user** via registration endpoint
4. ? **Use Swagger UI** for testing and data entry
5. ? **Export/Import data** using SQL scripts if needed

---

## Quick Start Script (PowerShell)

```powershell
# Set your API base URL
$baseUrl = "https://your-app.onrender.com"

# Register user
$registerResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body (@{
    fullName = "Admin User"
    email = "admin@example.com"
    password = "Admin123!"
} | ConvertTo-Json) -ContentType "application/json"

# Login
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body (@{
    email = "admin@example.com"
    password = "Admin123!"
} | ConvertTo-Json) -ContentType "application/json"

$token = $loginResponse.token

# Create store
$storeResponse = Invoke-RestMethod -Uri "$baseUrl/api/stores" -Method Post `
  -Headers @{ Authorization = "Bearer $token" } `
  -Body (@{
    storeName = "My First Store"
    storeDescription = "Sample store"
    storeAddress = "123 Main Street"
  } | ConvertTo-Json) -ContentType "application/json"

Write-Host "Store created: $($storeResponse.id)"
```

---

## Troubleshooting

### "Unauthorized" Error
- Ensure you're using the token from the login response
- Token format: `Bearer {token}` (with space)

### "Store ID is required" Error
- Add `X-Store-ID` header for store-scoped endpoints
- Get store ID from the create store response

### Database Empty After Deployment
- This is expected - data seeding has been removed
- Use one of the methods above to populate data

---

## Summary

? **Data seeding removed** - fixes Docker deployment crashes  
? **Use API endpoints** - recommended for production  
? **Migrations only** - handle schema, not data  
? **Manual or scripted** - data entry through proper channels  

**Your application is now deployment-ready without seeding issues!** ??
