# ?? Business Scenario Documentation
## E-Commerce Business Manager SaaS Platform

**Version**: 1.1 (with API Integration Guide)  
**Target Framework**: .NET 8  
**Database**: SQL Server  
**Architecture**: Clean Architecture (DDD)  
**Date**: December 18, 2024

---

## ?? Executive Summary

This platform is a **multi-tenant SaaS application** designed for e-commerce businesses to manage their operations across multiple sales channels, including:

- Product catalog management
- Customer relationship management (CRM)
- Order processing and fulfillment
- Marketing campaign management
- Social media automation (Facebook, Instagram, TikTok, YouTube)
- Team collaboration
- Chatbot order automation

---

## ?? Business Model Overview

### Core Concept

The platform operates on a **hierarchical business model**:

```
User (Account Owner)
  ??? Stores (Multiple)
      ??? Products
      ??? Customers
      ??? Orders
      ??? Campaigns
      ??? Social Media Accounts
      ??? Teams
      ??? Automation Rules
```

### Key Business Rules

1. **Multi-Store Support**: Each user can own and manage multiple stores
2. **Store Isolation**: Data is strictly segregated by store (enforced via `X-Store-ID` header)
   - **Implementation**: Store `currentStoreId` in localStorage
   - **API Integration**: Use HTTP interceptor to automatically add `X-Store-ID` header to all store-scoped requests
3. **Team Collaboration**: Store owners can invite team members with different roles *(placeholder for future implementation)*
4. **Omnichannel Sales**: Orders can come from:
   - Manual entry (admin)
   - Chatbot (Facebook Messenger)
   - Future: Web storefront, mobile app
5. **Campaign-Driven Marketing**: Products are promoted through scheduled social media campaigns

---

## ?? User Roles & Permissions

### 1. Store Owner
**Full Control** of their stores:
- ? Create/update/delete stores
- ? Manage all resources (products, orders, campaigns)
- ? Invite team members
- ? Connect social media accounts
- ? View all analytics

### 2. Team Moderator
**Management** permissions:
- ? Manage products and campaigns
- ? Process orders
- ? Manage customers
- ? Cannot delete store
- ? Cannot invite team members

### 3. Team Member
**Operational** permissions:
- ? View products and campaigns
- ? Process orders
- ? View customer information
- ? Cannot modify products
- ? Cannot create campaigns

---

## ?? Business Entities & Workflows

### 1. Store Management

#### Business Scenario
A user registers on the platform and creates their first store to start selling products online.

#### Entity: Store
```json
{
  "id": "guid",
  "storeName": "Fashion Hub",
  "storeDescription": "Trendy fashion for everyone",
  "storeAddress": "123 Main St, New York, NY",
  "ownerUserId": "user-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### Workflow
1. User registers ? JWT token issued
   - **API**: `POST /api/auth/register`
2. User creates store ? Becomes owner
   - **API**: `POST /api/stores` (GLOBAL - no X-Store-ID required)
3. Store ID stored in frontend (localStorage)
   - **Implementation**: `localStorage.setItem('currentStoreId', store.id)`
4. All subsequent requests include `X-Store-ID` header
   - **Implementation**: Use axios/fetch interceptor
5. User can switch between stores via dropdown
   - **API**: `GET /api/stores/my` (GLOBAL - returns all accessible stores)

#### Frontend Requirements
- **Store Selector Dropdown** (always visible in header)
  - **Data Source**: `GET /api/stores/my`
- **Store Creation Modal**
  - **Submit**: `POST /api/stores`
- **Store Settings Page**
  - **Load**: `GET /api/stores/{storeId}`
  - **Update**: `PUT /api/stores/{storeId}`

---

### 2. Product Catalog Management

#### Business Scenario
Store owner adds products to sell, which will be:
- Listed on their storefront
- Featured in marketing campaigns
- Ordered by customers
- Embedded for AI-powered search (via n8n webhook)

#### Entity: Product
```json
{
  "id": "guid",
  "productName": "Nike Air Max 2024",
  "productDescription": "Premium quality white nike shoes with advanced cushioning",
  "productPrice": 129.99,
  "inStock": true,
  "imageUrl": "https://example.com/product.jpg",
  "brand": "Nike",
  "condition": "New",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### Workflow
1. Owner adds product via **Product Form**
   - **API**: `POST /api/products` (STORE-SCOPED)
2. Product saved to database
3. **Automatic embedding** to n8n webhook (for AI search)
4. Product appears in catalog
   - **API**: `GET /api/products` (STORE-SCOPED)
5. Product can be assigned to campaigns
   - Used in campaign creation dropdown

#### Frontend Requirements
- **Product List Page** (grid/table view)
  - **Load**: `GET /api/products` (optional: `?inStockOnly=true`)
- **Product Creation Form**
  - **Submit**: `POST /api/products`
- **Product Edit Modal**
  - **Load**: `GET /api/products/{productId}`
  - **Update**: `PUT /api/products/{productId}`
- **Product Search Bar** (client-side filtering or future API)
- **Filter by In-Stock**
  - **API**: `GET /api/products?inStockOnly=true`
- **Product Image Upload** (future)

#### Special Features
- ? **Auto-embedding**: Product data sent to n8n after create/update
- ? **Campaign Assignment**: Products can be featured in marketing campaigns

---

### 3. Customer Management (CRM)

#### Business Scenario
Store tracks customer information for:
- Order processing
- Marketing outreach
- Customer support
- Chatbot conversations (via Facebook PSID)

#### Entity: Customer
```json
{
  "id": "guid",
  "customerName": "John Doe",
  "phone": "+1234567890",
  "billingAddress": "456 Customer St, LA, CA",
  "psid": "facebook_psid_value",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### Customer Sources
1. **Manual Entry**: Admin creates customer record
   - **API**: `POST /api/customers`
2. **Chatbot**: Auto-created from Facebook Messenger orders
   - **API**: `POST /api/orders/chatbot` (PUBLIC)
3. **Future**: Self-registration on storefront

#### Customer Deduplication Logic
When chatbot receives order:
1. Search by `PSID` (Facebook Page-Scoped ID) first
2. If not found, search by `Phone`
3. If found by phone, update PSID
4. If not found, create new customer (name defaults to "Anonymous" if empty)

#### Frontend Requirements
- **Customer List Page**
  - **Load**: `GET /api/customers` (STORE-SCOPED)
- **Customer Details Modal**
  - **Load**: `GET /api/customers/{customerId}`
- **Customer Creation Form**
  - **Submit**: `POST /api/customers`
- **Customer Edit Form**
  - **Update**: `PUT /api/customers/{customerId}`
- **Order History Tab** (per customer)
  - **Load**: `GET /api/orders/by-customer/{customerId}`

---

### 4. Order Management & Fulfillment

#### Business Scenario
Orders come from two sources:
1. **Admin Manual Entry**: Store staff creates orders
2. **Chatbot Orders**: Customers order via Facebook Messenger

All orders start as **"Pending"** and flow through a lifecycle:

```
Pending ? Accepted ? Shipped ? Delivered
        ? Rejected
        ? Cancelled
        ? Refunded
```

#### Entity: Order
```json
{
  "id": "guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Pending",
  "statusDisplayName": "Pending",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### Order Statuses (String-based)
- **Pending**: Awaiting admin approval
- **Accepted**: Approved by admin, ready to process
- **Shipped**: Order dispatched to customer
- **Delivered**: Order received by customer
- **Rejected**: Admin declined the order
- **Cancelled**: Customer or admin cancelled
- **Refunded**: Payment refunded

#### Order Workflow

**Manual Order**:
1. Admin selects customer
   - **API**: `GET /api/customers` (for dropdown)
2. Admin adds products
   - **API**: `GET /api/products` (for selection)
3. System calculates total
4. Order created as "Pending"
   - **API**: `POST /api/orders` (STORE-SCOPED)
5. Admin reviews and accepts/rejects
   - **Accept**: `PUT /api/orders/{orderId}/accept`
   - **Reject**: `PUT /api/orders/{orderId}/reject`

**Chatbot Order** (from Facebook Messenger):
1. Customer chats with bot
2. Bot extracts order details
3. n8n sends order to `/api/orders/chatbot` (public endpoint)
4. System resolves store via Facebook Page ID
5. System finds/creates customer
6. System matches products by name
7. Order created as "Pending"
8. Admin reviews in dashboard
   - **API**: `GET /api/orders?status=Pending`
9. Admin accepts ? Customer notified
   - **API**: `PUT /api/orders/{orderId}/accept`
10. Admin rejects ? Customer notified
    - **API**: `PUT /api/orders/{orderId}/reject`

#### Critical Business Rules
- ? Only "Pending" orders can be accepted/rejected
- ? Orders are never deleted (soft delete for audit)
- ? Chatbot orders always start as "Pending"
- ? If product not found in chatbot order ? skip item, continue order
- ? TotalPrice auto-calculated from OrderProducts

---

### 4.1. Order Product Management (Line Items)

#### Business Scenario
Order products represent the line items within an order. Each order product links a product to an order with specific quantity and price information. This allows tracking exactly what products were ordered, at what price, and in what quantity.

#### Entity: OrderProduct
```json
{
  "orderId": "order-guid",
  "productId": "product-guid",
  "productName": "Nike Air Max 2024",
  "quantity": 2,
  "unitPrice": 129.99,
  "totalPrice": 259.98
}
```

#### OrderProduct Workflow

**When Creating an Order**:
1. Admin selects customer
   - **API**: `GET /api/customers` (for dropdown)
2. Admin adds products one by one
   - **API**: `POST /api/OrderProduct` (STORE-SCOPED)
   - Each product added creates an OrderProduct record
3. System tracks each line item with:
   - Product reference
   - Quantity ordered
   - Unit price at time of order (locked-in price)
4. Order total auto-calculated from all OrderProducts

**Managing Order Products**:
1. **View order items**
   - **API**: `GET /api/OrderProduct/order/{orderId}`
2. **Update quantity**
   - **API**: `PUT /api/OrderProduct/{orderId}/product/{productId}`
3. **Remove item**
   - **API**: `DELETE /api/OrderProduct/{orderId}/product/{productId}`

#### OrderProduct Properties
- **OrderId**: Links to the parent order
- **ProductId**: Links to the product being ordered
- **ProductName**: Denormalized product name (for display)
- **Quantity**: Number of units ordered
- **UnitPrice**: Price per unit at time of order (immutable)
- **TotalPrice**: Calculated field (Quantity × UnitPrice)

#### Frontend Requirements
- **Order Details Page**
  - **Product List Table**
    - Columns: Product Name, Quantity, Unit Price, Total Price
    - **Load**: `GET /api/OrderProduct/order/{orderId}`
  - **Add Product Button**
    - Opens modal with product dropdown
    - **Products API**: `GET /api/products` (for selection)
    - **Submit**: `POST /api/OrderProduct`
  - **Quantity Edit Input**
    - Inline editing or modal
    - **Update**: `PUT /api/OrderProduct/{orderId}/product/{productId}`
  - **Remove Product Button**
    - Confirmation dialog
    - **Delete**: `DELETE /api/OrderProduct/{orderId}/product/{productId}`
  - **Order Total Display**
    - Auto-calculated sum of all line items
    - Updates after any OrderProduct change

- **Order Creation Form**
  - **Dynamic Product Table**
    - Add multiple products before order submission
    - Shows running total
  - **Product Search/Dropdown**
    - Filter products by name
    - Shows current price
  - **Quantity Input** (per product)
    - Validation: Must be > 0
    - Default: 1

#### Critical Business Rules
- ? **Price Lock-In**: `UnitPrice` captured at order creation time
  - Prevents retroactive price changes affecting existing orders
  - Reflects actual price customer agreed to pay
- ? **Duplicate Prevention**: Cannot add same product twice to an order
  - If product already exists, update quantity instead
  - Backend enforces this constraint
- ? **Automatic Total Calculation**: Order `TotalPrice` = sum of all OrderProduct totals
  - Frontend should display but not submit total
  - Backend calculates authoritative total
- ? **Quantity Validation**: Quantity must always be ? 1
  - Quantity = 0 ? Use DELETE instead
- ? **Cascade Delete**: Deleting an order ? All OrderProducts deleted
- ? **Product Reference Integrity**: OrderProduct links to Product
  - If product is deleted, OrderProduct keeps productName for history
  - ProductId becomes null (soft reference)

#### Business Validation Rules
- **Order must exist** before adding products
- **Product must exist** and belong to same store
- **Quantity must be positive** integer (? 1)
- **UnitPrice must be positive** decimal (? 0)
- **Cannot modify OrderProducts** on accepted/shipped orders (business rule enforcement optional)

#### API Integration Example

**Add Product to Order**:
```javascript
const addProductToOrder = async (orderId, productId, quantity, unitPrice) => {
  const response = await fetch('/api/OrderProduct', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      orderId: orderId,
      productId: productId,
      quantity: quantity,
      unitPrice: unitPrice
    })
  });
  return await response.json();
};
```

**Get Order Products**:
```javascript
const getOrderProducts = async (orderId) => {
  const response = await fetch(`/api/OrderProduct/order/${orderId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  return await response.json();
};
```

**Update Product Quantity**:
```javascript
const updateProductQuantity = async (orderId, productId, newQuantity) => {
  const response = await fetch(`/api/OrderProduct/${orderId}/product/${productId}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      quantity: newQuantity
    })
  });
  return await response.json();
};
```

**Remove Product from Order**:
```javascript
const removeProductFromOrder = async (orderId, productId) => {
  const response = await fetch(`/api/OrderProduct/${orderId}/product/${productId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  return response; // 204 No Content on success
};
```

#### Use Cases

**Use Case 1: Build Order with Multiple Products**
```
1. Admin creates order ? POST /api/orders
2. Admin adds Product A (qty: 2) ? POST /api/OrderProduct
3. Admin adds Product B (qty: 1) ? POST /api/OrderProduct
4. Admin adds Product C (qty: 5) ? POST /api/OrderProduct
5. System calculates total: (A.price × 2) + (B.price × 1) + (C.price × 5)
```

**Use Case 2: Customer Changes Mind (Edit Order)**
```
1. Admin views order items ? GET /api/OrderProduct/order/{orderId}
2. Customer wants 3 instead of 2 of Product A
3. Admin updates quantity ? PUT /api/OrderProduct/{orderId}/product/{A}
4. System recalculates order total
```

**Use Case 3: Remove Unwanted Item**
```
1. Customer decides not to buy Product C
2. Admin views order ? GET /api/OrderProduct/order/{orderId}
3. Admin removes Product C ? DELETE /api/OrderProduct/{orderId}/product/{C}
4. System recalculates order total (now only A + B)
```

**Use Case 4: Price Change After Order**
```
1. Order created with Product A at $100
2. Admin changes Product A price to $120
3. Existing order still shows $100 (price locked-in)
4. New orders will use $120 (current price)
```

#### Error Scenarios

**Scenario 1: Duplicate Product**
```
Request: POST /api/OrderProduct
Body: { orderId: "X", productId: "A", quantity: 2 }
Response: 400 Bad Request
{
  "message": "Product A is already in order X. Use UpdateQuantity instead."
}
```

**Scenario 2: Invalid Quantity**
```
Request: PUT /api/OrderProduct/{orderId}/product/{productId}
Body: { quantity: 0 }
Response: 400 Bad Request
{
  "message": "Quantity must be greater than 0."
}
```

**Scenario 3: Product Not in Order**
```
Request: PUT /api/OrderProduct/{orderId}/product/{productId}
Response: 404 Not Found
{
  "message": "Product {productId} not found in order {orderId}."
}
```

**Scenario 4: Order Not Found**
```
Request: POST /api/OrderProduct
Body: { orderId: "invalid-guid", productId: "A", quantity: 1 }
Response: 404 Not Found
{
  "message": "Order with ID invalid-guid not found."
}
```

---

### 5. Marketing Campaigns

#### Business Scenario
Store owners create marketing campaigns to promote products across social media platforms. Campaigns have stages and contain scheduled posts.

#### Campaign Stages
```
Draft ? InReview ? Scheduled ? Ready ? Published
```

#### Entity: Campaign
```json
{
  "id": "guid",
  "campaignName": "Summer Sale 2024",
  "campaignDescription": "50% off summer collection",
  "campaignStage": "Scheduled",
  "campaignBannerUrl": "https://example.com/banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-35, Fashion enthusiasts",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-08-31T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "product-guid",
  "assignedProductName": "Nike Air Max 2024",
  "createdByUserId": "user-guid",
  "createdByUserName": "John Doe",
  "storeId": "store-guid",
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-05-15T10:00:00Z"
}
```

#### Campaign Workflow
1. Owner creates campaign
   - **API**: `POST /api/campaigns` (STORE-SCOPED)
2. Assigns product to campaign (via lightweight dropdown of the store products)
   - **Products API**: `GET /api/products` (for dropdown)
3. Creates posts for campaign (one post page and a plus button to add another post to the campaign)
   - **API**: `POST /api/campaign-posts` (STORE-SCOPED)
4. Connects social media platforms
   - **API**: `GET /api/social-platforms/available-platforms` (GLOBAL)
   - **Connect**: `POST /api/social-platforms/facebook/connect` (STORE-SCOPED)
5. Schedules posts
   - **API**: `POST /api/campaign-posts` with `scheduledAt` field
6. Campaign moves to "Scheduled" stage
   - **API**: `PUT /api/campaigns/{campaignId}` (update stage)
7. Hangfire job publishes posts at scheduled time (automatic)
8. Campaign moves to "Published" stage

#### Frontend Requirements
- **Campaign List Page**
  - **Load**: `GET /api/campaigns` (STORE-SCOPED)
- **Campaign Creation Wizard**:
  - Step 1: Campaign details
    - **Submit**: `POST /api/campaigns`
  - Step 2: Product selection
    - **Load Products**: `GET /api/products`
  - Step 3: Create posts
    - **API**: `POST /api/campaign-posts`
  - Step 4: Schedule
    - **Update Campaign**: `PUT /api/campaigns/{campaignId}`
- **Campaign Dashboard** (analytics) - future
- **Campaign Stage Indicator**

---

### 6. Campaign Posts & Social Media Publishing

#### Business Scenario
Store owner creates posts within a campaign to be published across multiple social media platforms at scheduled times.

#### Entity: CampaignPost
```json
{
  "id": "guid",
  "campaignId": "campaign-guid",
  "campaignName": "Summer Sale 2024",
  "postCaption": "?? 50% OFF Summer Collection! Limited time offer!",
  "postImageUrl": "https://example.com/post-image.jpg",
  "scheduledAt": "2024-06-01T09:00:00Z",
  "publishStatus": "Pending",
  "publishedAt": null,
  "lastPublishError": null,
  "createdAt": "2024-05-20T10:00:00Z"
}
```

#### Post Creation Flow
1. Owner selects campaign
   - **API**: `GET /api/campaigns` (for dropdown)
   - **Implementation**: Lightweight dropdown with campaign name, stage, and date
2. Writes post caption
3. Uploads image (future: image upload endpoint)
4. Selects target platforms (checkboxes):
   - **CRITICAL**: Only show connected platforms
   - **API**: Call `GET /api/social-platforms/connected` (STORE-SCOPED) - **NOT YET IMPLEMENTED**
   - **Alternative**: Use `GetConnectedPlatformsByStoreIdAsync` from repository
   - ?? Facebook
   - ?? Instagram
   - ? TikTok
   - ? YouTube
5. Sets scheduled time
6. Clicks "Create Post"
7. Backend creates `CampaignPost` record
   - **API**: `POST /api/campaign-posts`
   - **Request Body**:
   ```json
   {
     "campaignId": "campaign-guid",
     "postCaption": "Post text...",
     "postImageUrl": "https://...",
     "scheduledAt": "2024-06-01T09:00:00Z",
     "platformIds": ["platform-guid-1", "platform-guid-2"]
   }
   ```
8. Backend creates `CampaignPostPlatform` records for each selected platform

#### Platform Selection Implementation

**Frontend Implementation**:
```javascript
// 1. Load connected platforms for checkboxes
const loadConnectedPlatforms = async () => {
  const response = await fetch('/api/social-platforms/connected', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  const platforms = await response.json();
  
  // platforms = [{ id: "guid", platformName: "Facebook", isConnected: true }, ...]
  
  // Render checkboxes (only connected platforms)
  platforms.forEach(platform => {
    if (platform.isConnected) {
      // Render checkbox
      createCheckbox(platform.id, platform.platformName);
    }
  });
};

// 2. Submit post with selected platforms
const createPost = async (postData) => {
  const selectedPlatformIds = getSelectedCheckboxIds(); // Array of platform GUIDs
  
  const response = await fetch('/api/campaign-posts', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      campaignId: postData.campaignId,
      postCaption: postData.caption,
      postImageUrl: postData.imageUrl,
      scheduledAt: postData.scheduledAt,
      platformIds: selectedPlatformIds // This is required!
    })
  });
  return await response.json();
};
```

**Backend Implementation Note**:
- The `CreateCampaignPostRequest` DTO must include `platformIds` property
- The `CampaignPostService.CreatePostAsync` method automatically creates `CampaignPostPlatform` records for each platform ID
- Only platforms with `IsConnected = true` for the current store should be selectable

#### Platform-Specific Posts (CampaignPostPlatform)
```json
{
  "id": "guid",
  "campaignPostId": "post-guid",
  "platformId": "platform-guid",
  "platformName": "Facebook",
  "externalPostId": "fb_post_123456",
  "publishStatus": "Published",
  "scheduledAt": "2024-06-01T09:00:00Z",
  "publishedAt": "2024-06-01T09:00:15Z",
  "errorMessage": null
}
```

#### Publishing Workflow
1. **Hangfire Background Job** runs every 1 minute (automatic)
2. Finds posts with `scheduledAt <= now` and `publishStatus = Pending`
3. For each post, publish to all selected platforms
4. Facebook Publisher uses Graph API
5. Updates `publishStatus` to "Published" or "Failed"
6. Stores `externalPostId` from platform
7. Records any errors

#### Publish Statuses
- **Pending**: Not yet published
- **Publishing**: Currently being published
- **Published**: Successfully published to all platforms
- **Failed**: Publishing failed (see errorMessage)

#### Frontend Requirements
- **Post Creation Form**
  - **Submit**: `POST /api/campaign-posts`
- **Platform Selection Checkboxes**
  - **Load Connected Platforms**: `GET /api/social-platforms/connected` *(needs implementation)* OR query by store ID from backend
  - **Important**: Only render checkboxes for platforms where `isConnected = true`
- **Date-Time Picker** (readable format: "Dec 18, 2024, 9:00 AM")
- **Campaign Dropdown** (lightweight, fast)
  - **Load**: `GET /api/campaigns`
- **Post List Page** (per campaign)
  - **Load**: `GET /api/campaign-posts` (filter by campaign client-side or add query param)
- **Post Status Indicator**
- **Retry Failed Posts Button** - future
- **View on Platform Link** (using externalPostId) - future

---

### 7. Social Media Platform Management

#### Business Scenario
Store owner connects their social media accounts to enable automated posting.

#### Supported Platforms
- ? **Facebook** (OAuth integration complete)
- ?? **Instagram** (planned)
- ?? **TikTok** (planned)
- ?? **YouTube** (planned)

#### Entity: SocialPlatform
```json
{
  "id": "guid",
  "platformName": "Facebook",
  "externalPageID": "927669027091667",
  "pageName": "Fashion Hub Official",
  "accessToken": "encrypted_token_value",
  "isConnected": true,
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": "2024-01-15T10:00:00Z"
}
```

#### Facebook Connection Flow
1. Owner clicks "Connect Facebook"
2. Facebook OAuth modal opens
3. User authorizes app
4. Facebook returns access token + page ID
5. Frontend sends to `/api/social-platforms/facebook/connect`
   - **API**: `POST /api/social-platforms/facebook/connect` (STORE-SCOPED)
6. Backend validates token
7. Backend stores connection
8. Platform appears as "Connected"

#### Platform Uses
- **Campaign Post Publishing**: Auto-post to connected accounts
- **Chatbot Order Resolution**: Facebook Page ID ? Store ID mapping

#### Frontend Requirements
- **Platform List Page**
  - **Load**: `GET /api/social-platforms` (STORE-SCOPED) - *(needs implementation)*
  - **Alternative**: Query connected platforms for current store
- **Connect Platform Buttons**
  - **Facebook**: `POST /api/social-platforms/facebook/connect`
- **OAuth Modal** (Facebook SDK)
- **Platform Status Indicators**
- **Disconnect/Reconnect Actions**
  - **Disconnect**: `PUT /api/social-platforms/{connectionId}/disconnect`
- **Platform Dropdown** (available platforms API)
  - **API**: `GET /api/social-platforms/available-platforms` (GLOBAL)

**API for Platform Checkboxes in Post Creation**:
```javascript
// Get connected platforms for current store
GET /api/social-platforms/connected // NOT YET IMPLEMENTED

// Alternative: Backend should add this endpoint
// Returns only platforms where isConnected = true for current store
```

---

### 8. Team Collaboration

#### Business Scenario
Store owner invites team members to help manage the store with different permission levels.

#### Team Roles
- **Owner**: Full control (assigned to creator)
- **Moderator**: Can manage products, campaigns, orders
- **Member**: Can view and process orders only

#### Entity: Team
```json
{
  "id": "guid",
  "teamName": "Marketing Team",
  "description": "Handles social media campaigns",
  "storeId": "store-guid",
  "storeName": "Fashion Hub",
  "memberCount": 5,
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### Team Member
```json
{
  "teamId": "team-guid",
  "userId": "user-guid",
  "userName": "Jane Doe",
  "userEmail": "jane@example.com",
  "role": "Moderator",
  "joinedAt": "2024-01-16T10:00:00Z"
}
```

#### Team Workflow
1. Owner creates team
   - **API**: `POST /api/teams` (STORE-SCOPED)
2. Owner invites users by email
   - **API**: `POST /api/teams/{teamId}/members` (STORE-SCOPED)
3. Backend sends invitation (future)
4. User accepts invitation (future)
5. User assigned role
6. User gains access to store resources

#### Frontend Requirements
- **Team List Page**
  - **Load**: `GET /api/teams` (STORE-SCOPED)
- **Team Creation Modal**
  - **Submit**: `POST /api/teams`
- **Member List**
  - **Load**: `GET /api/teams/{teamId}/members`
- **Invite Member Form**
  - **Submit**: `POST /api/teams/{teamId}/members`
- **Role Assignment Dropdown**
- **Remove Member Action**
  - **Delete**: `DELETE /api/teams/{teamId}/members/{userId}`

---

### 9. Chatbot Integration (n8n)

#### Business Scenario
Customers order products via Facebook Messenger chatbot, which integrates with the platform through n8n workflow.

#### Chatbot Order Flow

**Customer Side (Facebook Messenger)**:
1. Customer: "I want to order Nike shoes"
2. Bot: "What size?"
3. Customer: "Size 10, quantity 2"
4. Bot: "What's your phone and address?"
5. Customer provides details
6. Bot: "Order placed! Total: $259.98"

**Backend Side**:
1. n8n receives conversation
2. n8n extracts order details
3. n8n sends to `/api/orders/chatbot` (public, no auth)
   - **API**: `POST /api/orders/chatbot` (PUBLIC - no auth required)
4. Payload:
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
5. Backend resolves store from `pageId`
6. Backend finds/creates customer
7. Backend matches products by name
8. Backend creates order with Status = "Pending"
9. Admin receives notification (frontend polling or WebSockets)
   - **API**: `GET /api/orders?status=Pending`
10. Admin accepts order
    - **API**: `PUT /api/orders/{orderId}/accept`
11. n8n notifies customer (via webhook callback)

#### Business Rules
- ? Store resolved via Facebook `pageId`
- ? Customer deduplication by PSID then Phone
- ? Product matched by name (case-insensitive LIKE)
- ? Missing products skipped (order continues)
- ? Order always created as "Pending"
- ? Admin must accept/reject

#### Frontend Requirements
- **Pending Orders Dashboard Widget**
  - **Load**: `GET /api/orders?status=Pending` (poll every 30 seconds)
- **Chatbot Order Badge** (indicator)
  - Count of pending chatbot orders
- **Quick Accept/Reject Buttons**
  - **Accept**: `PUT /api/orders/{orderId}/accept`
  - **Reject**: `PUT /api/orders/{orderId}/reject`
- **Customer Auto-Link** (click to view customer)
  - Navigate to: `/customers/{customerId}`

---

### 10. Automation & Background Jobs

#### Business Scenario
Platform runs automated tasks in the background using Hangfire.

#### Automation Tasks

**1. Post Publishing**
- **Frequency**: Every 1 minute
- **Job**: `PlatformPublisherJob`
- **Action**: Publish scheduled posts to social media
- **(Automatic - no frontend interaction needed)**

**2. Product Embedding**
- **Trigger**: Product create/update
- **Action**: Send product data to n8n webhook
- **Use Case**: AI-powered product search
- **(Automatic - fires after product API calls)**

**3. Order Notifications** (planned)
- **Trigger**: Order status change
- **Action**: Send email/SMS to customer

#### Entity: AutomationTask
```json
{
  "id": "guid",
  "taskType": "AutoResponse",
  "cronExpression": "*/5 * * * *",
  "isActive": true,
  "lastRunDate": "2024-01-15T10:00:00Z",
  "relatedCampaignPostId": "post-guid",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

#### Frontend Requirements
- **Automation Dashboard**
  - **Load**: `GET /api/automation-tasks` (STORE-SCOPED)
- **Task List**
- **Enable/Disable Toggles**
  - **Update**: `PUT /api/automation-tasks/{taskId}`
- **Task Logs Viewer** - future

---

## ?? Authentication & Authorization

### Authentication Flow

**Registration**:
1. User submits email, password, full name
   - **API**: `POST /api/auth/register`
2. Backend creates user account
3. JWT token returned
4. Token stored in localStorage
   - **Implementation**: `localStorage.setItem('authToken', response.token)`

**Login**:
1. User submits email, password
   - **API**: `POST /api/auth/login`
2. Backend validates credentials
3. JWT token returned
4. Token stored in localStorage

**Token Usage**:
```javascript
headers: {
  'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
  'X-Store-ID': `${localStorage.getItem('currentStoreId')}`,
  'Content-Type': 'application/json'
}
```

**Interceptor Setup** (Axios Example):
```javascript
axios.interceptors.request.use(config => {
  const token = localStorage.getItem('authToken');
  const storeId = localStorage.getItem('currentStoreId');
  
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  
  // Only add X-Store-ID for store-scoped endpoints
  if (storeId && isStoreScopedEndpoint(config.url)) {
    config.headers['X-Store-ID'] = storeId;
  }
  
  return config;
});

function isStoreScopedEndpoint(url) {
  const globalEndpoints = ['/api/auth', '/api/users', '/api/stores', '/api/teams/my', '/api/social-platforms/available-platforms'];
  return !globalEndpoints.some(endpoint => url.includes(endpoint));
}
```

### Endpoint Types

**GLOBAL Endpoints** (no `X-Store-ID` required):
- `/api/auth/*` - Authentication
- `/api/users/*` - User management
- `/api/stores/*` - Store management
- `/api/teams/my` - User's teams
- `/api/social-platforms/available-platforms` - Platform dropdown

**STORE-SCOPED Endpoints** (require `X-Store-ID`):
- `/api/products` - Product management
- `/api/customers` - Customer management
- `/api/orders` - Order management (except `/chatbot`)
- `/api/campaigns` - Campaign management
- `/api/campaign-posts` - Post management
- `/api/social-platforms` (except `/available-platforms`)
- `/api/teams` (except `/my`)

**PUBLIC Endpoints** (no auth):
- `/api/orders/chatbot` - Receive orders from n8n

---

## ?? Data Relationships

### Core Relationships

```
User
 ?? owns multiple Stores
 ?? belongs to multiple Teams

Store
 ?? has many Products
 ?? has many Customers
 ?? has many Orders
 ?? has many Campaigns
 ?? has many SocialPlatforms
 ?? has many Teams

Campaign
 ?? has many CampaignPosts
 ?? assigned one Product (featured)

CampaignPost
 ?? has many CampaignPostPlatforms (one per platform)

Order
 ?? belongs to one Customer
 ?? has many OrderProducts (line items)

Customer
 ?? has many Orders

Product
 ?? has many OrderProducts
 ?? has many Campaigns (featured in)
```

### Cascade Delete Rules

**Store Deleted** ? Cascade deletes:
- Products
- Customers
- Orders
- Campaigns
- Campaign Posts
- Social Platforms
- Teams
- Automation Tasks

**Campaign Deleted** ? Cascade deletes:
- Campaign Posts
- Campaign Post Platforms

**Order Deleted** ? Cascade deletes:
- Order Products

---

## ?? Frontend Application Structure

### Recommended Pages

```
/
??? /auth
?   ??? /login ? POST /api/auth/login
?   ??? /register ? POST /api/auth/register
?
??? /dashboard (overview)
?   ??? Pending orders widget ? GET /api/orders?status=Pending
?   ??? Revenue chart
?   ??? Recent activities
?
??? /stores
?   ??? Store selector dropdown ? GET /api/stores/my
?   ??? /create ? POST /api/stores
?
??? /products
?   ??? /list ? GET /api/products
?   ??? /create ? POST /api/products
?   ??? /:id/edit ? PUT /api/products/{id}
?
??? /customers
?   ??? /list ? GET /api/customers
?   ??? /create ? POST /api/customers
?   ??? /:id/view ? GET /api/customers/{id}
?
??? /orders
?   ??? /list ? GET /api/orders?status={status}
?   ??? /create ? POST /api/orders
?   ??? /:id/details ? GET /api/orders/{id}
?
??? /campaigns
?   ??? /list ? GET /api/campaigns
?   ??? /create ? POST /api/campaigns
?   ??? /:id/details ? GET /api/campaigns/{id}
?       ??? /posts ? GET /api/campaign-posts (filter by campaign)
?
??? /social-platforms
?   ??? /list ? GET /api/social-platforms (needs endpoint)
?   ??? /connect/facebook ? POST /api/social-platforms/facebook/connect
?
??? /teams
?   ??? /list ? GET /api/teams
?   ??? /create ? POST /api/teams
?   ??? /:id/members ? GET /api/teams/{id}/members
?
??? /automation
?   ??? /tasks ? GET /api/automation-tasks
?
??? /settings
    ??? /profile ? GET /api/users/me, PUT /api/users/{id}
    ??? /store ? GET /api/stores/{id}, PUT /api/stores/{id}
```

---

## ?? API Integration Examples

### Product Management

**Create Product**:
```javascript
const createProduct = async (productData) => {
  const response = await fetch('/api/products', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      productName: productData.name,
      productDescription: productData.description,
      productPrice: productData.price,
      inStock: productData.inStock
    })
  });
  return await response.json();
};
```

**Get Products**:
```javascript
const getProducts = async (inStockOnly = false) => {
  const url = new URL('/api/products', API_BASE);
  if (inStockOnly) url.searchParams.set('inStockOnly', 'true');
  
  const response = await fetch(url, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  return await response.json();
};
```

### Order Management

**Get Pending Orders**:
```javascript
const getPendingOrders = async () => {
  const response = await fetch('/api/orders?status=Pending', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  return await response.json();
};
```

**Accept Order**:
```javascript
const acceptOrder = async (orderId) => {
  const response = await fetch(`/api/orders/${orderId}/accept`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  return await response.json();
};
```

### Campaign Post Management

**Create Campaign Post with Platform Selection**:
```javascript
const createPost = async (postData) => {
  // First, load connected platforms
  const platformsResponse = await fetch('/api/social-platforms/connected', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  const platforms = await platformsResponse.json();
  
  // User selects platforms via checkboxes
  const selectedPlatformIds = postData.selectedPlatforms; // Array of GUIDs
  
  const response = await fetch('/api/campaign-posts', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      campaignId: postData.campaignId,
      postCaption: postData.caption,
      postImageUrl: postData.imageUrl,
      scheduledAt: postData.scheduledAt,
      platformIds: selectedPlatformIds // Required for multi-platform publishing
    })
  });
  return await response.json();
};
```

---

## ?? Complete API Reference

For detailed API documentation with request/response examples, refer to:
- **FRONTEND_INTEGRATION_GUIDE.md** - Complete API reference

---

## ?? Complete API Endpoints Reference

### Quick Reference
- **Base URL**: `https://api.yourdomain.com/api` or `http://localhost:5000/api`
- **Authentication**: Bearer JWT Token (except public endpoints)
- **Store Context**: `X-Store-ID` header (for store-scoped endpoints)
- **Content-Type**: `application/json`

---

### ?? Authentication Endpoints

#### 1. Register User
**Endpoint**: `POST /api/auth/register`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: None

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

**Success Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenExpiration": "2024-12-19T10:00:00Z"
}
```

**Error Responses**:
- **400 Bad Request**: Validation errors (e.g., passwords don't match, weak password)
- **409 Conflict**: Email already exists

---

#### 2. Login
**Endpoint**: `POST /api/auth/login`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: None

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
```json
{
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Success Response** (200 OK):
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenExpiration": "2024-12-19T10:00:00Z"
}
```

**Error Responses**:
- **400 Bad Request**: Invalid credentials
- **404 Not Found**: User not found

---

#### 3. Logout
**Endpoint**: `POST /api/auth/logout`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**: None

**Success Response** (200 OK):
```json
{
  "message": "Logged out successfully"
}
```

**Error Responses**:
- **401 Unauthorized**: Invalid or expired token

---

### ?? Store Management Endpoints

#### 1. Get My Stores
**Endpoint**: `GET /api/stores/my`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
```

**Query Parameters**: None

**Success Response** (200 OK):
```json
[
  {
    "id": "store-guid-1",
    "storeName": "Fashion Hub",
    "storeDescription": "Trendy fashion for everyone",
    "storeAddress": "123 Main St, New York, NY",
    "ownerUserId": "user-guid",
    "createdAt": "2024-01-15T10:00:00Z"
  },
  {
    "id": "store-guid-2",
    "storeName": "Tech Store",
    "storeDescription": "Latest gadgets",
    "storeAddress": "456 Tech Ave, San Francisco, CA",
    "ownerUserId": "user-guid",
    "createdAt": "2024-02-20T10:00:00Z"
  }
]
```

**Error Responses**:
- **401 Unauthorized**: Invalid token
- **500 Internal Server Error**: Database error

---

#### 2. Get Store by ID
**Endpoint**: `GET /api/stores/{storeId}`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
```

**Path Parameters**:
- `storeId` (GUID, required): Store identifier

**Success Response** (200 OK):
```json
{
  "id": "store-guid",
  "storeName": "Fashion Hub",
  "storeDescription": "Trendy fashion for everyone",
  "storeAddress": "123 Main St, New York, NY",
  "ownerUserId": "user-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Error Responses**:
- **404 Not Found**: Store not found

---

#### 3. Create Store
**Endpoint**: `POST /api/stores`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**:
```json
{
  "storeName": "Fashion Hub",
  "storeDescription": "Trendy fashion for everyone",
  "storeAddress": "123 Main St, New York, NY"
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-store-guid",
  "storeName": "Fashion Hub",
  "storeDescription": "Trendy fashion for everyone",
  "storeAddress": "123 Main St, New York, NY",
  "ownerUserId": "current-user-guid",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

**Error Responses**:
- **400 Bad Request**: Validation errors (missing required fields)
- **401 Unauthorized**: Invalid token

---

#### 4. Update Store
**Endpoint**: `PUT /api/stores/{storeId}`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters**:
- `storeId` (GUID, required): Store identifier

**Request Body**:
```json
{
  "storeName": "Fashion Hub Updated",
  "storeDescription": "Premium fashion for everyone",
  "storeAddress": "123 Main St, New York, NY"
}
```

**Success Response** (200 OK):
```json
{
  "id": "store-guid",
  "storeName": "Fashion Hub Updated",
  "storeDescription": "Premium fashion for everyone",
  "storeAddress": "123 Main St, New York, NY",
  "ownerUserId": "user-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Error Responses**:
- **404 Not Found**: Store not found
- **403 Forbidden**: User is not the owner

---

### ?? Product Management Endpoints

#### 1. Get All Products
**Endpoint**: `GET /api/products`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Query Parameters**:
- `inStockOnly` (boolean, optional): Filter only in-stock products

**Success Response** (200 OK):
```json
[
  {
    "id": "product-guid-1",
    "productName": "Nike Air Max 2024",
    "productDescription": "Premium quality white nike shoes",
    "productPrice": 129.99,
    "inStock": true,
    "imageUrl": "https://example.com/product.jpg",
    "brand": "Nike",
    "condition": "New",
    "storeId": "store-guid",
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

**Error Responses**:
- **401 Unauthorized**: Invalid token
- **400 Bad Request**: Missing X-Store-ID header

---

#### 2. Get Product by ID
**Endpoint**: `GET /api/products/{productId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `productId` (GUID, required): Product identifier

**Success Response** (200 OK):
```json
{
  "id": "product-guid",
  "productName": "Nike Air Max 2024",
  "productDescription": "Premium quality white nike shoes",
  "productPrice": 129.99,
  "inStock": true,
  "imageUrl": "https://example.com/product.jpg",
  "brand": "Nike",
  "condition": "New",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Error Responses**:
- **404 Not Found**: Product not found

---

#### 3. Create Product
**Endpoint**: `POST /api/products`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "productName": "Nike Air Max 2024",
  "productDescription": "Premium quality white nike shoes with advanced cushioning",
  "productPrice": 129.99,
  "inStock": true,
  "imageUrl": "https://example.com/product.jpg",
  "brand": "Nike",
  "condition": "New"
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-product-guid",
  "productName": "Nike Air Max 2024",
  "productDescription": "Premium quality white nike shoes with advanced cushioning",
  "productPrice": 129.99,
  "inStock": true,
  "imageUrl": "https://example.com/product.jpg",
  "brand": "Nike",
  "condition": "New",
  "storeId": "store-guid",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

**Side Effects**:
- Automatically sends product data to n8n embedding webhook (for AI search)

**Error Responses**:
- **400 Bad Request**: Validation errors (e.g., negative price, missing name)

---

#### 4. Update Product
**Endpoint**: `PUT /api/products/{productId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `productId` (GUID, required): Product identifier

**Request Body**:
```json
{
  "productName": "Nike Air Max 2024 Updated",
  "productPrice": 119.99,
  "inStock": false
}
```

**Success Response** (200 OK):
```json
{
  "id": "product-guid",
  "productName": "Nike Air Max 2024 Updated",
  "productDescription": "Premium quality white nike shoes with advanced cushioning",
  "productPrice": 119.99,
  "inStock": false,
  "imageUrl": "https://example.com/product.jpg",
  "brand": "Nike",
  "condition": "New",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Side Effects**:
- Automatically re-sends product data to n8n embedding webhook

**Error Responses**:
- **404 Not Found**: Product not found

---

### ?? Customer Management Endpoints

#### 1. Get All Customers
**Endpoint**: `GET /api/customers`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Success Response** (200 OK):
```json
[
  {
    "id": "customer-guid-1",
    "customerName": "John Doe",
    "phone": "+1234567890",
    "billingAddress": "456 Customer St, LA, CA",
    "psid": "facebook_psid_value",
    "storeId": "store-guid",
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

---

#### 2. Get Customer by ID
**Endpoint**: `GET /api/customers/{customerId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `customerId` (GUID, required): Customer identifier

**Success Response** (200 OK):
```json
{
  "id": "customer-guid",
  "customerName": "John Doe",
  "phone": "+1234567890",
  "billingAddress": "456 Customer St, LA, CA",
  "psid": "facebook_psid_value",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

**Error Responses**:
- **404 Not Found**: Customer not found

---

#### 3. Create Customer
**Endpoint**: `POST /api/customers`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "customerName": "John Doe",
  "phone": "+1234567890",
  "billingAddress": "456 Customer St, LA, CA",
  "psid": "facebook_psid_value"
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-customer-guid",
  "customerName": "John Doe",
  "phone": "+1234567890",
  "billingAddress": "456 Customer St, LA, CA",
  "psid": "facebook_psid_value",
  "storeId": "store-guid",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

**Error Responses**:
- **400 Bad Request**: Validation errors

---

#### 4. Update Customer
**Endpoint**: `PUT /api/customers/{customerId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `customerId` (GUID, required): Customer identifier

**Request Body**:
```json
{
  "customerName": "John Doe Updated",
  "phone": "+1234567890",
  "billingAddress": "789 New St, LA, CA"
}
```

**Success Response** (200 OK):
```json
{
  "id": "customer-guid",
  "customerName": "John Doe Updated",
  "phone": "+1234567890",
  "billingAddress": "789 New St, LA, CA",
  "psid": "facebook_psid_value",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

---

### ?? Order Management Endpoints

#### 1. Get All Orders
**Endpoint**: `GET /api/orders`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Query Parameters**:
- `status` (OrderStatus enum, optional): Filter by status
  - Valid values: `Pending`, `Accepted`, `Shipped`, `Delivered`, `Rejected`, `Cancelled`, `Refunded`

**Success Response** (200 OK):
```json
[
  {
    "id": "order-guid-1",
    "customerId": "customer-guid",
    "customerName": "John Doe",
    "totalPrice": 259.98,
    "status": "Pending",
    "statusDisplayName": "Pending",
    "storeId": "store-guid",
    "createdAt": "2024-12-18T10:00:00Z"
  }
]
```

**Example**: Get only pending orders
```
GET /api/orders?status=Pending
```

---

#### 2. Get Order by ID
**Endpoint**: `GET /api/orders/{orderId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `orderId` (GUID, required): Order identifier

**Success Response** (200 OK):
```json
{
  "id": "order-guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Pending",
  "statusDisplayName": "Pending",
  "storeId": "store-guid",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

---

#### 3. Get Orders by Customer
**Endpoint**: `GET /api/orders/by-customer/{customerId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `customerId` (GUID, required): Customer identifier

**Success Response** (200 OK):
```json
{
  "id": "order-guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Delivered",
  "statusDisplayName": "Delivered",
  "storeId": "store-guid",
  "createdAt": "2024-11-18T10:00:00Z"
}
```

---

#### 4. Create Order
**Endpoint**: `POST /api/orders`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "customerId": "customer-guid",
  "items": [
    {
      "productId": "product-guid-1",
      "quantity": 2
    },
    {
      "productId": "product-guid-2",
      "quantity": 1
    }
  ]
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-order-guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Pending",
  "statusDisplayName": "Pending",
  "storeId": "store-guid",
  "createdAt": "2024-12-18T10:00:00Z"
}
```

**Business Rules**:
- Order automatically created with `Status = "Pending"`
- `TotalPrice` auto-calculated from products

---

#### 5. Update Order
**Endpoint**: `PUT /api/orders/{orderId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `orderId` (GUID, required): Order identifier

**Request Body**:
```json
{
  "status": "Shipped"
}
```

**Success Response** (200 OK):
```json
{
  "id": "order-guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Shipped",
  "statusDisplayName": "Shipped",
  "storeId": "store-guid",
  "createdAt": "2024-12-17T10:00:00Z"
}
```

---

#### 6. Accept Order
**Endpoint**: `PUT /api/orders/{orderId}/accept`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `orderId` (GUID, required): Order identifier

**Request Body**: None

**Success Response** (200 OK):
```json
{
  "id": "order-guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Accepted",
  "statusDisplayName": "Accepted",
  "storeId": "store-guid",
  "createdAt": "2024-12-17T10:00:00Z"
}
```

**Business Rules**:
- Only orders with `Status = "Pending"` can be accepted
- Status changes from `Pending` ? `Accepted`

**Error Responses**:
- **400 Bad Request**: Order is not in Pending status
- **404 Not Found**: Order not found

---

#### 7. Reject Order
**Endpoint**: `PUT /api/orders/{orderId}/reject`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `orderId` (GUID, required): Order identifier

**Request Body**: None

**Success Response** (200 OK):
```json
{
  "id": "order-guid",
  "customerId": "customer-guid",
  "customerName": "John Doe",
  "totalPrice": 259.98,
  "status": "Rejected",
  "statusDisplayName": "Rejected",
  "storeId": "store-guid",
  "createdAt": "2024-12-17T10:00:00Z"
}
```

**Business Rules**:
- Only orders with `Status = "Pending"` can be rejected
- Status changes from `Pending` ? `Rejected`
- Rejected orders are preserved (not deleted)

**Error Responses**:
- **400 Bad Request**: Order is not in Pending status
- **404 Not Found**: Order not found

---

#### 8. Chatbot Order (PUBLIC)
**Endpoint**: `POST /api/orders/chatbot`  
**Type**: PUBLIC (no authentication required)  
**Authentication**: None

**Headers**:
```
Content-Type: application/json
```

**Request Body**:
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
    },
    {
      "productName": "adidas running shoes",
      "quantity": 1
    }
  ],
  "pageId": "927669027091667"
}
```

**Success Response** (201 Created):
```json
{
  "success": true,
  "message": "Order created successfully with 2 item(s)",
  "orderId": "newly-created-order-guid",
  "status": "Pending",
  "totalPrice": 259.98,
  "customerName": "mahmoud"
}
```

**Business Rules**:
- Store resolved via Facebook `pageId`
- Customer deduplication:
  1. Search by `PSID` first
  2. If not found, search by `Phone`
  3. If found by phone, update PSID
  4. If not found, create new customer (name = "Anonymous" if empty)
- Product matching by name (case-insensitive LIKE)
- Missing products skipped (order continues with available products)
- Order created with `Status = "Pending"`

**Error Responses**:
- **400 Bad Request**: Invalid Facebook Page ID, validation errors
- **500 Internal Server Error**: Database error

---

### ?? Campaign Management Endpoints

#### 1. Get All Campaigns
**Endpoint**: `GET /api/campaigns`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Success Response** (200 OK):
```json
[
  {
    "id": "campaign-guid-1",
    "campaignName": "Summer Sale 2024",
    "campaignDescription": "50% off summer collection",
    "campaignStage": "Scheduled",
    "campaignBannerUrl": "https://example.com/banner.jpg",
    "goal": "Sales",
    "targetAudience": "18-35, Fashion enthusiasts",
    "scheduledStartAt": "2024-06-01T00:00:00Z",
    "scheduledEndAt": "2024-08-31T23:59:59Z",
    "isSchedulingEnabled": true,
    "assignedProductId": "product-guid",
    "assignedProductName": "Nike Air Max 2024",
    "createdByUserId": "user-guid",
    "createdByUserName": "John Doe",
    "storeId": "store-guid",
    "createdAt": "2024-05-01T10:00:00Z",
    "updatedAt": "2024-05-15T10:00:00Z"
  }
]
```

---

#### 2. Get Campaign by ID
**Endpoint**: `GET /api/campaigns/{campaignId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `campaignId` (GUID, required): Campaign identifier

**Success Response** (200 OK):
```json
{
  "id": "campaign-guid",
  "campaignName": "Summer Sale 2024",
  "campaignDescription": "50% off summer collection",
  "campaignStage": "Scheduled",
  "campaignBannerUrl": "https://example.com/banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-35, Fashion enthusiasts",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-08-31T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "product-guid",
  "assignedProductName": "Nike Air Max 2024",
  "createdByUserId": "user-guid",
  "createdByUserName": "John Doe",
  "storeId": "store-guid",
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-05-15T10:00:00Z"
}
```

---

#### 3. Create Campaign
**Endpoint**: `POST /api/campaigns`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "campaignName": "Summer Sale 2024",
  "campaignDescription": "50% off summer collection",
  "campaignStage": "Draft",
  "campaignBannerUrl": "https://example.com/banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-35, Fashion enthusiasts",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-08-31T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "product-guid"
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-campaign-guid",
  "campaignName": "Summer Sale 2024",
  "campaignDescription": "50% off summer collection",
  "campaignStage": "Draft",
  "campaignBannerUrl": "https://example.com/banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-35, Fashion enthusiasts",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-08-31T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "product-guid",
  "assignedProductName": "Nike Air Max 2024",
  "createdByUserId": "current-user-guid",
  "createdByUserName": "John Doe",
  "storeId": "store-guid",
  "createdAt": "2024-12-18T10:00:00Z",
  "updatedAt": "2024-12-18T10:00:00Z"
}
```

**Campaign Stages**:
- `Draft`, `InReview`, `Scheduled`, `Ready`, `Published`

---

#### 4. Update Campaign
**Endpoint**: `PUT /api/campaigns/{campaignId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `campaignId` (GUID, required): Campaign identifier

**Request Body**:
```json
{
  "campaignName": "Summer Sale 2024 Extended",
  "campaignStage": "Scheduled",
  "scheduledEndAt": "2024-09-30T23:59:59Z"
}
```

**Success Response** (200 OK):
```json
{
  "id": "campaign-guid",
  "campaignName": "Summer Sale 2024 Extended",
  "campaignDescription": "50% off summer collection",
  "campaignStage": "Scheduled",
  "campaignBannerUrl": "https://example.com/banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-35, Fashion enthusiasts",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-09-30T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "product-guid",
  "assignedProductName": "Nike Air Max 2024",
  "createdByUserId": "user-guid",
  "createdByUserName": "John Doe",
  "storeId": "store-guid",
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-12-18T10:00:00Z"
}
```

---

### ?? Campaign Post Endpoints

#### 1. Get All Campaign Posts
**Endpoint**: `GET /api/campaign-posts`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Success Response** (200 OK):
```json
[
  {
    "id": "post-guid-1",
    "campaignId": "campaign-guid",
    "campaignName": "Summer Sale 2024",
    "postCaption": "?? 50% OFF Summer Collection! Limited time offer!",
    "postImageUrl": "https://example.com/post-image.jpg",
    "scheduledAt": "2024-06-01T09:00:00Z",
    "publishStatus": "Published",
    "publishedAt": "2024-06-01T09:00:15Z",
    "lastPublishError": null,
    "createdAt": "2024-05-20T10:00:00Z"
  }
]
```

---

#### 2. Get Campaign Post by ID
**Endpoint**: `GET /api/campaign-posts/{postId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `postId` (GUID, required): Post identifier

**Success Response** (200 OK):
```json
{
  "id": "post-guid",
  "campaignId": "campaign-guid",
  "campaignName": "Summer Sale 2024",
  "postCaption": "?? 50% OFF Summer Collection! Limited time offer!",
  "postImageUrl": "https://example.com/post-image.jpg",
  "scheduledAt": "2024-06-01T09:00:00Z",
  "publishStatus": "Published",
  "publishedAt": "2024-06-01T09:00:15Z",
  "lastPublishError": null,
  "createdAt": "2024-05-20T10:00:00Z"
}
```

---

#### 3. Create Campaign Post
**Endpoint**: `POST /api/campaign-posts`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "campaignId": "campaign-guid",
  "postCaption": "?? 50% OFF Summer Collection! Limited time offer!",
  "postImageUrl": "https://example.com/post-image.jpg",
  "scheduledAt": "2024-06-01T09:00:00Z",
  "platformIds": ["platform-guid-1", "platform-guid-2"]
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-post-guid",
  "campaignId": "campaign-guid",
  "campaignName": "Summer Sale 2024",
  "postCaption": "?? 50% OFF Summer Collection! Limited time offer!",
  "postImageUrl": "https://example.com/post-image.jpg",
  "scheduledAt": "2024-06-01T09:00:00Z",
  "publishStatus": "Pending",
  "publishedAt": null,
  "lastPublishError": null,
  "createdAt": "2024-12-18T10:00:00Z"
}
```

**Business Rules**:
- `platformIds` array must contain GUIDs of connected platforms
- Backend automatically creates `CampaignPostPlatform` records for each selected platform
- Post created with `publishStatus = "Pending"`
- Hangfire job publishes post at scheduled time

**Error Responses**:
- **400 Bad Request**: Invalid campaign ID, missing platformIds
- **404 Not Found**: Campaign not found

---

#### 4. Update Campaign Post
**Endpoint**: `PUT /api/campaign-posts/{postId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `postId` (GUID, required): Post identifier

**Request Body**:
```json
{
  "postCaption": "?? 50% OFF - EXTENDED! Summer Collection Sale!",
  "scheduledAt": "2024-06-02T09:00:00Z"
}
```

**Success Response** (200 OK):
```json
{
  "id": "post-guid",
  "campaignId": "campaign-guid",
  "campaignName": "Summer Sale 2024",
  "postCaption": "?? 50% OFF - EXTENDED! Summer Collection Sale!",
  "postImageUrl": "https://example.com/post-image.jpg",
  "scheduledAt": "2024-06-02T09:00:00Z",
  "publishStatus": "Pending",
  "publishedAt": null,
  "lastPublishError": null,
  "createdAt": "2024-05-20T10:00:00Z"
}
```

---

### ?? Social Platform Endpoints

#### 1. Get Available Platforms (Dropdown)
**Endpoint**: `GET /api/social-platforms/available-platforms`  
**Type**: GLOBAL (no X-Store-ID required)  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
```

**Success Response** (200 OK):
```json
[
  {
    "value": 0,
    "name": "Facebook",
    "displayName": "Facebook",
    "isOAuthEnabled": true
  },
  {
    "value": 1,
    "name": "Instagram",
    "displayName": "Instagram",
    "isOAuthEnabled": false
  },
  {
    "value": 2,
    "name": "TikTok",
    "displayName": "TikTok",
    "isOAuthEnabled": false
  },
  {
    "value": 3,
    "name": "YouTube",
    "displayName": "YouTube",
    "isOAuthEnabled": false
  }
]
```

**Use Case**: Platform selection dropdown

---

#### 2. Get Platform by ID
**Endpoint**: `GET /api/social-platforms/{connectionId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `connectionId` (GUID, required): Platform connection identifier

**Success Response** (200 OK):
```json
{
  "id": "platform-guid",
  "platformName": "Facebook",
  "externalPageID": "927669027091667",
  "pageName": "Fashion Hub Official",
  "accessToken": "encrypted_token_value",
  "isConnected": true,
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": "2024-01-15T10:00:00Z"
}
```

---

#### 3. Connect Facebook Platform
**Endpoint**: `POST /api/social-platforms/facebook/connect`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "code": "facebook_oauth_code",
  "redirectUri": "https://yourapp.com/callback"
}
```

**Success Response** (200 OK):
```json
{
  "id": "platform-guid",
  "platformName": "Facebook",
  "externalPageID": "927669027091667",
  "pageName": "Fashion Hub Official",
  "accessToken": "encrypted_token",
  "isConnected": true,
  "storeId": "store-guid",
  "createdAt": "2024-12-18T10:00:00Z",
  "updatedAt": "2024-12-18T10:00:00Z"
}
```

**Business Rules**:
- Exchanges OAuth code for access token
- Validates token with Facebook
- Stores platform connection
- If platform already exists, updates it

---

#### 4. Disconnect Platform
**Endpoint**: `PUT /api/social-platforms/{connectionId}/disconnect`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `connectionId` (GUID, required): Platform connection identifier

**Success Response** (200 OK):
```json
{
  "id": "platform-guid",
  "platformName": "Facebook",
  "externalPageID": "927669027091667",
  "pageName": "Fashion Hub Official",
  "accessToken": "",
  "isConnected": false,
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z",
  "updatedAt": "2024-12-18T10:00:00Z"
}
```

---

### ?? Team Management Endpoints

#### 1. Get All Teams
**Endpoint**: `GET /api/teams`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Success Response** (200 OK):
```json
[
  {
    "id": "team-guid-1",
    "teamName": "Marketing Team",
    "description": "Handles social media campaigns",
    "storeId": "store-guid",
    "storeName": "Fashion Hub",
    "memberCount": 5,
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

---

#### 2. Get Team Members
**Endpoint**: `GET /api/teams/{teamId}/members`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Path Parameters**:
- `teamId` (GUID, required): Team identifier

**Success Response** (200 OK):
```json
[
  {
    "teamId": "team-guid",
    "userId": "user-guid-1",
    "userName": "Jane Doe",
    "userEmail": "jane@example.com",
    "role": "Moderator",
    "joinedAt": "2024-01-16T10:00:00Z"
  }
]
```

---

#### 3. Create Team
**Endpoint**: `POST /api/teams`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Request Body**:
```json
{
  "teamName": "Marketing Team",
  "description": "Handles social media campaigns"
}
```

**Success Response** (201 Created):
```json
{
  "id": "newly-created-team-guid",
  "teamName": "Marketing Team",
  "description": "Handles social media campaigns",
  "storeId": "store-guid",
  "storeName": "Fashion Hub",
  "memberCount": 0,
  "createdAt": "2024-12-18T10:00:00Z"
}
```

---

### ?? Automation Task Endpoints

#### 1. Get All Automation Tasks
**Endpoint**: `GET /api/automation-tasks`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
```

**Success Response** (200 OK):
```json
[
  {
    "id": "task-guid-1",
    "taskType": "AutoResponse",
    "cronExpression": "*/5 * * * *",
    "isActive": true,
    "lastRunDate": "2024-12-18T10:00:00Z",
    "relatedCampaignPostId": "post-guid",
    "storeId": "store-guid",
    "createdAt": "2024-01-15T10:00:00Z"
  }
]
```

---

#### 2. Update Automation Task
**Endpoint**: `PUT /api/automation-tasks/{taskId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `taskId` (GUID, required): Task identifier

**Request Body**:
```json
{
  "isActive": false
}
```

**Success Response** (200 OK):
```json
{
  "id": "task-guid",
  "taskType": "AutoResponse",
  "cronExpression": "*/5 * * * *",
  "isActive": false,
  "lastRunDate": "2024-12-18T10:00:00Z",
  "relatedCampaignPostId": "post-guid",
  "storeId": "store-guid",
  "createdAt": "2024-01-15T10:00:00Z"
}
```

---

## ?? Missing API Endpoints (Action Required)

### 1. Get Connected Platforms for Store
**Endpoint Needed**: `GET /api/social-platforms/connected`  
**Type**: STORE-SCOPED  
**Purpose**: Return only connected platforms (`isConnected = true`) for current store  
**Used By**: Campaign post creation (platform checkboxes)

**Expected Response**:
```json
[
  {
    "id": "platform-guid-1",
    "platformName": "Facebook",
    "isConnected": true,
    "externalPageID": "927669027091667",
    "pageName": "Fashion Hub Official"
  }
]
```

**Workaround**: Frontend can call `GET /api/social-platforms` and filter `isConnected = true` client-side

### 2. Get All Social Platforms for Store
**Endpoint Needed**: `GET /api/social-platforms`  
**Type**: STORE-SCOPED  
**Purpose**: Return all platforms (connected + disconnected) for current store  
**Used By**: Social platforms management page

**Expected Response**:
```json
[
  {
    "id": "platform-guid-1",
    "platformName": "Facebook",
    "isConnected": true,
    "externalPageID": "927669027091667",
    "pageName": "Fashion Hub Official",
    "storeId": "store-guid",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  },
  {
    "id": "platform-guid-2",
    "platformName": "Instagram",
    "isConnected": false,
    "externalPageID": null,
    "pageName": null,
    "storeId": "store-guid",
    "createdAt": "2024-02-20T10:00:00Z",
    "updatedAt": "2024-02-20T10:00:00Z"
  }
]
```

**Current Status**: Repository method exists (`GetByStoreIdAsync`), but controller endpoint missing

---

**End of Business Scenario Documentation**

This document provides the complete business context with API integration details needed to build a frontend application that integrates seamlessly with the Business Manager SaaS platform.
