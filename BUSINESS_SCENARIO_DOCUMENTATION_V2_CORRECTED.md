# ?? Business Scenario Documentation (Corrected for Frontend Generation)
## E-Commerce Business Manager SaaS Platform

**Version**: 2.1 (Copilot-Optimized - Final)  
**Target Framework**: .NET 8  
**Database**: SQL Server  
**Architecture**: Clean Architecture (DDD)  
**Date**: December 19, 2024  
**Purpose**: **Complete specification for AI-powered frontend code generation**

---

## ?? Executive Summary

This platform is a **multi-tenant SaaS application** designed for e-commerce businesses to manage their operations across multiple sales channels, including:

- Product catalog management
- Customer relationship management (CRM)
- Order processing and fulfillment
- Marketing campaign management
- Social media automation (Facebook, Instagram, TikTok, YouTube)
- Team collaboration (future)
- **Chatbot order automation (via external n8n integration)**

---

## ?? Business Model Overview

### Core Concept

The platform operates on a **hierarchical business model**:

```
User (Account Owner)
  ??? Stores (Multiple) + Team Memberships (Multiple - placeholder)
      ??? Products
      ??? Customers
      ??? Orders
      ??? Campaigns
      ??? Social Media Accounts
      ??? Teams (placeholder)
      ??? Automation Rules (future)
```

### Key Business Rules

1. **Multi-Store Support**: Each user can own and manage multiple stores
2. **Store Isolation**: Data is strictly segregated by store (enforced via `X-Store-ID` header)
   - **Implementation**: Store `currentStoreId` in localStorage
   - **API Integration**: Use HTTP interceptor to automatically add `X-Store-ID` header to all store-scoped requests
3. **Team Collaboration**: *(Placeholder for future implementation - not currently functional)*
   - **User can belong to multiple teams** (read-only for now)
   - **Teams provide access to stores** (placeholder - no permission enforcement yet)
4. **Omnichannel Sales**: Orders can come from:
   - Manual entry (admin via frontend)
   - **Chatbot (Facebook Messenger via n8n - backend receives finalized orders only)**
   - Future: Web storefront, mobile app
5. **Campaign-Driven Marketing**: Products are promoted through scheduled social media campaigns

---

## ?? Homepage & Store Selection (CRITICAL)

### Homepage Flow

**User lands on homepage** (`/` or `/home`)

**Scenario A: User Has Stores/Teams**
```
????????????????????????????????????????????????????????
?  ?? Business Manager                    [?? EN ?]   ?
????????????????????????????????????????????????????????
?                                                       ?
?  My Stores (3)                                       ?
?  ???????????  ???????????  ???????????  ?????????? ?
?  ? Fashion ?  ?  Tech   ?  ? Grocery ?  ?   +    ? ?
?  ?   Hub   ?  ?  Store  ?  ?  Market ?  ?  New   ? ?
?  ? ?? Owner?  ? ?? Owner?  ? ?? Owner?  ? Store  ? ?
?  ???????????  ???????????  ???????????  ?????????? ?
?                                                       ?
?  My Teams (2) - Placeholder                          ?
?  ???????????  ???????????                           ?
?  ?Marketing?  ?  Sales  ?                           ?
?  ?  Team   ?  ?  Team   ?                           ?
?  ? Fashion ?  ?  Tech   ?                           ?
?  ?   Hub   ?  ?  Store  ?                           ?
?  ???????????  ???????????                           ?
????????????????????????????????????????????????????????
```

**Scenario B: User Has NO Stores/Teams**
```
????????????????????????????????????????????????????????
?  ?? Business Manager                    [?? EN ?]   ?
????????????????????????????????????????????????????????
?                                                       ?
?            ?? Welcome to Business Manager            ?
?                                                       ?
?  ???????????????????????????????????????????????    ?
?  ?  ?? Start a Store Now                       ?    ?
?  ?                                              ?    ?
?  ?  Get your business up and running online!   ?    ?
?  ?  Manage products, orders, and campaigns.    ?    ?
?  ?                                              ?    ?
?  ?  [? Create Your First Store]               ?    ?
?  ???????????????????????????????????????????????    ?
?                                                       ?
?  ???????????????????????????????????????????????    ?
?  ?  ?? Join a Team (Placeholder)               ?    ?
?  ?                                              ?    ?
?  ?  Get an invitation to join a team and       ?    ?
?  ?  start managing online stores together!     ?    ?
?  ?                                              ?    ?
?  ?  [?? Invitation Link] (Disabled)            ?    ?
?  ???????????????????????????????????????????????    ?
????????????????????????????????????????????????????????
```

### Homepage APIs
- **Load Stores**: `GET /api/stores/my` (GLOBAL)
- **Load Teams**: `GET /api/teams/my` (GLOBAL) - *Returns placeholder data for now*
- **Create Store**: `POST /api/stores` (GLOBAL)

### Homepage Behavior
1. **On Load**:
   - Fetch `GET /api/stores/my` and `GET /api/teams/my`
   - If both empty ? Show "Start a Store" + "Join a Team" empty states
   - If stores exist ? Show store grid with "+ New Store" card
   - If teams exist ? Show team list (placeholder - click does nothing)

2. **Store Selection**:
   - User clicks store card
   - Store ID saved to `localStorage.setItem('currentStoreId', storeId)`
   - Redirect to `/dashboard` (store-scoped)
   - Header shows store selector dropdown

3. **New Store Card**:
   - Always visible in store grid (same size as store cards)
   - Labeled "+ New Store"
   - Opens create store modal

4. **Team Cards** (Placeholder):
   - Show team name + parent store name
   - **Click does nothing** (disabled state)
   - Tooltip: "Team collaboration coming soon"

### Design Requirements
- **Professional & Alive**: Modern, vibrant UI with smooth animations
- **Elegant**: Clean layout, good spacing, subtle shadows
- **Bilingual**: Full support for English (LTR) and Arabic (RTL)
- **Language Switcher**: Dropdown in navbar (?? EN ? / ?? AR ?)
- **RTL Support**: Layout flips for Arabic, icons remain logical
- **Responsive**: Works on desktop, tablet, mobile
- **Color Palette**: Follows design system (Primary: Blue, Success: Green, etc.)

---

## ?? CRITICAL CLARIFICATIONS FOR FRONTEND GENERATION

### ?? A. Chatbot Architecture (MUST UNDERSTAND)

**IMPORTANT**: The backend **DOES NOT** handle Facebook Messenger messages directly.

**Actual Architecture**:
```
Facebook Messenger
    ?
n8n Webhook (handles all conversation logic)
    ?
n8n extracts order details
    ?
n8n sends finalized order to ? POST /api/orders/chatbot (PUBLIC)
    ?
Backend persists order and manages lifecycle
```

**What the Backend Does**:
- ? Receives **finalized orders** from n8n via `POST /api/orders/chatbot`
- ? Resolves StoreID from Facebook PageID
- ? Creates/finds customers
- ? Matches products by name
- ? Creates orders with Status = "Pending"
- ? Manages order lifecycle (Accept/Reject/Ship/Deliver)

**What the Backend Does NOT Do**:
- ? Receive raw Facebook messages
- ? Handle chat conversations
- ? Forward messages to chatbot
- ? Provide websocket/real-time chat UI

**Frontend Implications**:
- **DO NOT** generate chat UI components
- **DO NOT** generate message handling logic
- **DO NOT** generate websocket connections for chat
- **DO** generate pending orders dashboard
- **DO** generate order accept/reject buttons
- **DO** poll `GET /api/orders?status=Pending` for new chatbot orders

---

### ?? B. Store Resolution for Chatbot Orders

**CRITICAL**: Frontend **never** sends `StoreID` for chatbot orders.

**Store Resolution Flow** (Backend Internal):
```
1. n8n sends: { pageId: "927669027091667", customer: {...}, items: [...] }
2. Backend queries: SocialPlatforms table WHERE ExternalPageID = pageId
3. Backend extracts: platform.StoreId
4. Backend creates order with resolved StoreId
```

**Frontend Implications**:
- Chatbot orders arrive without frontend involvement
- Frontend only sees completed orders in pending list
- Frontend never needs to resolve Facebook Page ? Store

**?? CHATBOT ENDPOINT RULE (CRITICAL)**:
```
POST /api/orders/chatbot is EXCLUSIVELY used by n8n
```

**Frontend MUST**:
- ? **NEVER** call `/api/orders/chatbot` endpoint
- ? **NEVER** generate forms for chatbot order creation
- ? **NEVER** generate services/hooks for chatbot order submission
- ? **NEVER** generate UI for creating chatbot orders

**Frontend ONLY Consumes Orders Via**:
- ? `GET /api/orders` - View orders
- ? `GET /api/orders?status=Pending` - Poll pending orders
- ? `PUT /api/orders/{id}/accept` - Accept order
- ? `PUT /api/orders/{id}/reject` - Reject order
- ? `GET /api/orders/{id}` - View order details

**Why This Matters**:
- Chatbot endpoint has NO authentication (public for n8n)
- Chatbot endpoint has different payload structure (pageId, psid)
- Frontend calling this would bypass business logic
- Creates security risk and data integrity issues

---

### ?? C. Order Status Enum (CANONICAL)

**Use these EXACT values** (stored as strings in database):

```typescript
enum OrderStatus {
  Pending = "Pending",       // Chatbot order awaiting admin approval
  Accepted = "Accepted",     // Admin approved
  Shipped = "Shipped",       // Order dispatched
  Delivered = "Delivered",   // Order received by customer
  Rejected = "Rejected",     // Admin declined
  Cancelled = "Cancelled",   // Cancelled by customer or admin
  Refunded = "Refunded"      // Payment refunded
}
```

**DO NOT USE**:
- ? "Processing" (does not exist)
- ? "Approved" (use "Accepted")
- ? "Declined" (use "Rejected")

**API Returns**:
```json
{
  "status": "Pending",           // Exact enum value
  "statusDisplayName": "Pending" // Human-readable (same as status)
}
```

**Order Source Detection**:
- **Preferred Method** (if `orderSource` field exists in future):
  ```json
  {
    "orderSource": "Chatbot" | "Manual"
  }
  ```
  Frontend should rely on this if available.

- **Current Method** (infer from customer PSID):
  - If `customer.psid` exists ? Likely chatbot order
  - Show "Chatbot Order" badge
  - **Note**: This is inference-based; if `orderSource` field is added, use that instead.

---

### ?? D. Campaign Stage Enum (CANONICAL)

**Use these EXACT values** (stored as enum in database, returned as strings):

```typescript
enum CampaignStage {
  Draft = "Draft",           // Initial creation
  InReview = "InReview",     // Under review (future)
  Scheduled = "Scheduled",   // Posts scheduled
  Ready = "Ready",           // Ready to publish
  Published = "Published"    // Published
}
```

**Campaign Workflow** (enforced by backend):
```
Draft ? InReview ? Scheduled ? Ready ? Published
```

---

### ?? E. Social Platform Management (CRITICAL)

**Current Implementation Status**:
- ? **Facebook**: Fully integrated (OAuth, posting)
- ?? **Instagram**: Planned (not available)
- ?? **TikTok**: Planned (not available)
- ?? **YouTube**: Planned (not available)

**Missing Endpoint** (Action Required):
```
GET /api/social-platforms/connected
```
**Status**: NOT YET IMPLEMENTED

**Frontend Assumption**:
- **Until this endpoint exists**, frontend should assume **only Facebook is available**.
- **Do not generate** complex multi-platform UI yet.
- **Platform checkboxes in post creation**: Show only Facebook (hardcoded) until API is ready.
- **Alternative workaround**: Query individual platform by ID if needed.

**Future-Ready Approach**:
```typescript
// Hardcode for now
const availablePlatforms = [
  { id: 'facebook-guid', platformName: 'Facebook', isConnected: true }
];

// When API is ready, replace with:
const response = await api.get('/social-platforms/connected');
const availablePlatforms = response.data;
```

---

### ?? F. Team Collaboration (PLACEHOLDER - CRITICAL)

**Status**: **UI placeholders ONLY**, no functional backend.

**Copilot MUST**:
- Generate **UI-only placeholders** (disabled buttons, grayed-out cards)
- **NO API calls** to team invitation or permission endpoints
- Show tooltips: "Team collaboration coming soon"
- **Homepage**: Display teams user belongs to (read-only list)
- **Team cards**: Click does nothing (disabled state)

**What to Generate**:
- ? Team list on homepage (from `GET /api/teams/my`)
- ? Team card UI (name, store, disabled state)
- ? Placeholder messages ("Feature coming soon")

**What NOT to Generate**:
- ? Team invitation forms
- ? Role assignment UI
- ? Permission management
- ? API calls to `/api/teams/{id}/members` or similar

---

## ?? Campaign Creation Flow (EXPLICIT SEQUENCING)

**CRITICAL**: This is a **multi-step wizard**, NOT a single form.

### ?? Step 1: Create Campaign (Draft State)

**Page**: `/campaigns/create`  
**API**: `POST /api/campaigns`

**User Actions**:
1. Enter campaign name
2. Enter campaign description
3. Select goal (Sales, Brand Awareness, etc.)
4. Define target audience
5. **Optionally** assign featured product (dropdown from `GET /api/products`)
6. Click "Save as Draft"

**Request**:
```json
{
  "campaignName": "Summer Sale 2024",
  "campaignDescription": "50% off summer collection",
  "campaignStage": "Draft",  // Always Draft initially
  "goal": "Sales",
  "targetAudience": "18-35, Fashion enthusiasts",
  "assignedProductId": "product-guid" // Optional
}
```

**Response**:
```json
{
  "id": "campaign-guid",
  "campaignName": "Summer Sale 2024",
  "campaignStage": "Draft",
  "createdAt": "2024-06-01T10:00:00Z"
}
```

**Frontend Actions**:
- Store `campaignId` in state
- Navigate to `/campaigns/{campaignId}/posts`

---

### ?? Step 2: Add Campaign Posts

**Page**: `/campaigns/{campaignId}/posts`  
**API**: `POST /api/campaign-posts` (called multiple times)

**User Actions**:
1. View existing posts for this campaign
2. Click **"+ Add Post"** button
3. Post creation modal opens:
   - Enter post caption
   - Upload/enter image URL
   - **Select target platforms** (checkboxes: Facebook, Instagram, etc.)
     - **Load platforms**: `GET /api/social-platforms/connected`
     - **Show only connected platforms** (`isConnected = true`)
   - **Optional**: Set scheduled time
4. Click "Create Post"
5. Repeat for additional posts

**Request** (per post):
```json
{
  "campaignId": "campaign-guid",
  "postCaption": "?? 50% OFF Summer Collection!",
  "postImageUrl": "https://example.com/image.jpg",
  "scheduledAt": "2024-06-15T09:00:00Z", // Optional
  "platformIds": ["facebook-platform-guid", "instagram-platform-guid"]
}
```

**Backend Behavior**:
- Creates `CampaignPost` record
- Creates `CampaignPostPlatform` records for each selected platform
- Post status = "Pending" (awaits scheduled time)

**Frontend UI**:
```
???????????????????????????????????????????
? Campaign: Summer Sale 2024              ?
? Status: Draft                           ?
???????????????????????????????????????????
? Posts (3)                               ?
? ??????????????????????????????????????? ?
? ? Post 1: "50% OFF!"                  ? ?
? ? Scheduled: Jun 15, 2024 9:00 AM    ? ?
? ? Platforms: Facebook, Instagram      ? ?
? ??????????????????????????????????????? ?
? ??????????????????????????????????????? ?
? ? Post 2: "Limited Time Offer"       ? ?
? ? Scheduled: Jun 16, 2024 9:00 AM    ? ?
? ? Platforms: Facebook                 ? ?
? ??????????????????????????????????????? ?
?                                         ?
? [+ Add Another Post]                    ?
?                                         ?
? [< Back]  [Next: Publish Options >]    ?
???????????????????????????????????????????
```

---

### ?? Step 3: Final Action (Publish or Schedule)

**Page**: `/campaigns/{campaignId}/publish`  
**API**: `PUT /api/campaigns/{campaignId}`

**User Choices**:

#### Option A: Publish Now
**User Action**: Click "Publish Now"  
**Request**:
```json
{
  "campaignStage": "Ready"
}
```

**Backend Behavior**:
- Campaign stage ? "Ready"
- Hangfire immediately processes all pending posts
- Posts publish to selected platforms
- Campaign stage ? "Published"

---

#### Option B: Schedule Campaign
**User Action**: Click "Schedule Campaign"  
**Request**:
```json
{
  "campaignStage": "Scheduled",
  "isSchedulingEnabled": true,
  "scheduledStartAt": "2024-06-15T00:00:00Z",
  "scheduledEndAt": "2024-08-31T23:59:59Z"
}
```

**Backend Behavior**:
- Campaign stage ? "Scheduled"
- Hangfire job runs every 1 minute
- Posts publish at their individual `scheduledAt` times
- When all posts published ? Campaign stage ? "Published"

---

### ?? Frontend State Machine

```typescript
// Campaign Wizard State
interface CampaignWizardState {
  step: 1 | 2 | 3;
  campaignId: string | null;
  campaignData: {
    name: string;
    description: string;
    stage: CampaignStage;
  } | null;
  posts: CampaignPost[];
}

// Step 1: Create Campaign
async function createCampaign(data) {
  const response = await api.post('/campaigns', data);
  setState({ step: 2, campaignId: response.id, campaignData: response });
}

// Step 2: Add Posts (repeatable)
async function addPost(postData) {
  await api.post('/campaign-posts', { ...postData, campaignId: state.campaignId });
  loadPosts(); // Refresh post list
}

// Step 3: Finalize
async function publishNow() {
  await api.put(`/campaigns/${state.campaignId}`, { campaignStage: 'Ready' });
  navigate('/campaigns'); // Done
}

async function scheduleCampaign(schedule) {
  await api.put(`/campaigns/${state.campaignId}`, {
    campaignStage: 'Scheduled',
    isSchedulingEnabled: true,
    ...schedule
  });
  navigate('/campaigns'); // Done
}
```

---

## ?? User Roles & Permissions

### 1. Store Owner
**Full Control** of their stores:
- ? Create/update/delete stores
- ? Manage all resources (products, orders, campaigns)
- ? Connect social media accounts
- ? View all analytics
- ?? **Invite team members** (placeholder - not functional yet)

### 2. Team Moderator *(Future)*
**Management** permissions:
- ?? Manage products and campaigns
- ?? Process orders
- ?? Manage customers
- ? Cannot delete store
- ? Cannot invite team members

### 3. Team Member *(Future)*
**Operational** permissions:
- ?? View products and campaigns
- ?? Process orders
- ?? View customer information
- ? Cannot modify products
- ? Cannot create campaigns

---

## ?? Business Entities & Workflows

### 1. Store Management

#### Business Scenario
A user registers on the platform and creates their first store to start selling products online. **After registration, user lands on homepage to select or create a store**.

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
2. **User redirected to homepage** (`/` or `/home`)
   - **Load stores**: `GET /api/stores/my`
   - **Load teams**: `GET /api/teams/my` (placeholder)
3. If no stores ? Show "Start a Store" empty state
4. User clicks "Create Your First Store" or "+ New Store"
   - Store creation modal opens
5. User creates store ? Becomes owner
   - **API**: `POST /api/stores` (GLOBAL - no X-Store-ID required)
6. User clicks store card ? Store ID stored in frontend (localStorage)
   - **Implementation**: `localStorage.setItem('currentStoreId', store.id)`
7. User redirected to `/dashboard` (store-scoped)
8. All subsequent requests include `X-Store-ID` header
   - **Implementation**: Use axios/fetch interceptor
9. User can switch between stores via dropdown in header
   - **API**: `GET /api/stores/my` (GLOBAL - returns all accessible stores)

#### Frontend Requirements
- **Homepage** (store/team selection)
  - **Load Stores**: `GET /api/stores/my`
  - **Load Teams**: `GET /api/teams/my` (placeholder)
  - **Store Grid**: Show owned stores + "+ New Store" card
  - **Team List**: Show teams (read-only, disabled)
  - **Empty State**: "Start a Store" + "Join a Team" (placeholder)
- **Store Selector Dropdown** (in header after store selection)
  - **Data Source**: `GET /api/stores/my`
  - **Always visible** in header (after store selected)
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
- **Embedded for AI-powered search (automatic - via n8n webhook)**

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
3. **Backend automatically sends product data to n8n embedding webhook** (no frontend action needed)
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
- **Product Search Bar** (client-side filtering)
- **Filter by In-Stock**
  - **API**: `GET /api/products?inStockOnly=true`
- **Product Image Upload** (future)

#### Special Features
- ? **Auto-embedding**: Product data sent to n8n after create/update (automatic, no UI needed)
- ? **Campaign Assignment**: Products can be featured in marketing campaigns

---

### 3. Customer Management (CRM)

#### Business Scenario
Store tracks customer information for:
- Order processing
- Marketing outreach
- Customer support
- **Chatbot conversations (identified by Facebook PSID)**

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
1. **Manual Entry**: Admin creates customer record via frontend
   - **API**: `POST /api/customers`
2. **Chatbot**: Auto-created by backend from n8n chatbot orders
   - **API**: `POST /api/orders/chatbot` (PUBLIC, called by n8n)
   - Backend creates customer if not found
3. **Future**: Self-registration on storefront

#### Customer Deduplication Logic (Backend Only)
When chatbot order received:
1. Search by `PSID` (Facebook Page-Scoped ID) first
2. If not found, search by `Phone`
3. If found by phone, update PSID
4. If not found, create new customer (name defaults to "Anonymous" if empty)

**Frontend Implication**: Frontend never handles deduplication

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
Orders come from **two sources**:
1. **Admin Manual Entry**: Store staff creates orders via frontend
2. **Chatbot Orders**: Customers order via Facebook Messenger (handled by n8n, received as finalized orders)

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

#### Order Statuses (String-based - CANONICAL)
- **Pending**: Awaiting admin approval (initial state for all orders)
- **Accepted**: Approved by admin, ready to process
- **Shipped**: Order dispatched to customer
- **Delivered**: Order received by customer
- **Rejected**: Admin declined the order
- **Cancelled**: Customer or admin cancelled
- **Refunded**: Payment refunded

#### Order Source Detection (UPDATED)

**Preferred Method** (if backend adds `orderSource` field in future):
```json
{
  "id": "order-guid",
  "orderSource": "Chatbot" | "Manual",
  "status": "Pending"
}
```
**Frontend should rely on this field if available.**

**Current Method** (infer from customer PSID):
- If `customer.psid` exists and not empty ? Likely chatbot order
- Show "Chatbot Order" badge
- **Note**: This is inference-based. If `orderSource` field is added to API, switch to using that instead.

**Implementation**:
```typescript
function isChatbotOrder(order: Order): boolean {
  // Preferred: Check explicit field (future)
  if (order.orderSource) {
    return order.orderSource === 'Chatbot';
  }
  
  // Fallback: Infer from customer PSID (current)
  return order.customer?.psid != null && order.customer.psid !== '';
}
```

#### Order Workflow

##### Manual Order (Admin)
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

##### Chatbot Order (from n8n)
**Frontend Perspective**:
1. **Background**: Customer orders via Facebook Messenger (n8n handles conversation)
2. **Background**: n8n extracts order details and sends to backend
3. **Frontend**: New order appears in pending orders list (via polling or refresh)
   - **API**: `GET /api/orders?status=Pending`
4. **Frontend**: Admin sees order with "Chatbot Order" badge
5. **Frontend**: Admin reviews order details
6. **Frontend**: Admin clicks "Accept" or "Reject"
   - **Accept API**: `PUT /api/orders/{orderId}/accept`
   - **Reject API**: `PUT /api/orders/{orderId}/reject`
7. **Background**: Backend notifies n8n (n8n notifies customer)

**?? CRITICAL - Frontend NEVER Calls Chatbot Endpoint**:
- **Endpoint**: `POST /api/orders/chatbot` is **EXCLUSIVELY for n8n**
- **Frontend**: Must NEVER call this endpoint
- **Reason**: Public endpoint (no auth), different payload structure, security risk
- **Frontend Role**: Only consumes orders via GET/PUT endpoints above

**Chatbot Order API Flow** (for understanding only - frontend does NOT call this):
```json
POST /api/orders/chatbot (PUBLIC - no auth)
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

**Backend Resolves**:
- `pageId` ? SocialPlatform ? StoreId (no frontend involvement)
- Finds/creates customer
- Matches products by name
- Creates order with Status = "Pending"

#### Order Products (Line Items)
```json
{
  "orderId": "order-guid",
  "productId": "product-guid",
  "quantity": 2,
  "unitPrice": 129.99
}
```

#### Frontend Requirements
- **Order Dashboard** (pending orders counter)
  - **Load**: `GET /api/orders?status=Pending`
  - **Polling**: Every 30 seconds (to catch new chatbot orders)
- **Order List Page** (filter by status)
  - **Load**: `GET /api/orders` or `GET /api/orders?status={status}`
  - **Tabs**: All, Pending, Accepted, Shipped, Delivered
  - **Chatbot Badge**: Show if `isChatbotOrder(order) === true`
- **Order Details Modal**
  - **Load**: `GET /api/orders/{orderId}`
  - **Show**: Customer info, products, total price, status
  - **Chatbot Indicator**: Badge or icon if chatbot order
- **Order Creation Form** (manual orders only)
  - **Submit**: `POST /api/orders` (**NOT** `/api/orders/chatbot`)
  - **CRITICAL**: Frontend creates manual orders only, never chatbot orders
- **Accept/Reject Buttons** (for pending orders)
  - **Accept**: `PUT /api/orders/{orderId}/accept`
  - **Reject**: `PUT /api/orders/{orderId}/reject`
  - **Enabled only if**: `status === "Pending"`
- **Status Update Dropdown**
  - **Update**: `PUT /api/orders/{orderId}` (with new status)
- **Order Timeline** (status history) - future

### 4.1. Order Product Management (Line Items)

#### Business Scenario
Order products represent the **line items** within an order. Each order product links a product to an order with specific quantity and price information. This allows precise tracking of:
- What products were ordered
- At what price (locked-in at order time)
- In what quantity
- Line-by-line order composition

**Why This Matters**:
- **Price Lock-In**: Unit price captured at order creation prevents retroactive price changes
- **Order Auditing**: Complete history of what was sold and at what price
- **Inventory Tracking**: Know exactly which products were included in each order
- **Customer Service**: Show customers exact breakdown of their order

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

**Field Descriptions**:
- **orderId**: Links to parent Order entity
- **productId**: Links to Product entity
- **productName**: Denormalized for display (preserves product name even if product is deleted)
- **quantity**: Number of units ordered (minimum: 1)
- **unitPrice**: Price per unit **at time of order** (immutable after creation)
- **totalPrice**: Calculated field (`quantity × unitPrice`) - **read-only**, backend calculates

#### OrderProduct Workflow

**When Creating a Manual Order** (Admin):
1. Admin clicks "Create Order" button
2. Order creation form opens
3. Admin selects customer from dropdown
   - **API**: `GET /api/customers` (STORE-SCOPED)
4. Admin adds products **one by one**:
   - Click "+ Add Product" button
   - Select product from dropdown
     - **API**: `GET /api/products` (STORE-SCOPED)
   - Enter quantity
   - Unit price auto-filled from product's current price
   - **Submit**: `POST /api/OrderProduct` (STORE-SCOPED)
5. Each product added creates an **OrderProduct** record
6. System displays running total (sum of all line items)
7. Admin submits order
   - **API**: `POST /api/orders` (STORE-SCOPED)
8. Order created with Status = "Pending"

**Managing Existing Order Products**:
1. **View order items**:
   - **API**: `GET /api/OrderProduct/order/{orderId}` (STORE-SCOPED)
   - Returns array of all products in order
2. **Update product quantity**:
   - Admin clicks "Edit" on line item
   - Changes quantity
   - **API**: `PUT /api/OrderProduct/{orderId}/product/{productId}` (STORE-SCOPED)
   - Order total recalculates automatically
3. **Remove item from order**:
   - Admin clicks "Remove" on line item
   - Confirmation dialog appears
   - **API**: `DELETE /api/OrderProduct/{orderId}/product/{productId}` (STORE-SCOPED)
   - Order total recalculates automatically

#### API Endpoints (STORE-SCOPED)

**?? ROUTE NOTE**: Current controller uses `Route("api/[controller]")` which results in:
- **Current Route**: `/api/OrderProduct` (PascalCase - controller name)
- **Convention**: Should be `/api/order-products` (kebab-case plural)
- **Documentation**: Using current route until fixed

##### GET `/api/OrderProduct/order/{orderId}` - Get All Products in Order

**Purpose**: Retrieve all line items (products) for a specific order  
**Auth**: Required  
**Headers**: `X-Store-ID: {storeId}`  
**URL Params**: `orderId` (guid)

**Response 200**:
```json
[
  {
    "orderId": "order-guid",
    "productId": "product-guid-1",
    "productName": "Nike Air Max 2024",
    "quantity": 2,
    "unitPrice": 129.99,
    "totalPrice": 259.98
  },
  {
    "orderId": "order-guid",
    "productId": "product-guid-2",
    "productName": "Adidas Ultraboost",
    "quantity": 1,
    "unitPrice": 180.00,
    "totalPrice": 180.00
  }
]
```

**Response 404**: Order not found

**Frontend Usage**:
```typescript
async function getOrderProducts(orderId: string) {
  const response = await api.get(`/OrderProduct/order/${orderId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  return response.data; // Array of OrderProductDto
}
```

---

##### POST `/api/OrderProduct` - Add Product to Order

**Purpose**: Add a single product line item to an existing order  
**Auth**: Required  
**Headers**: `X-Store-ID: {storeId}`

**Request Body**:
```json
{
  "orderId": "order-guid",
  "productId": "product-guid",
  "quantity": 2,
  "unitPrice": 129.99
}
```

**Validation Rules**:
- ? `orderId`: Must exist
- ? `productId`: Must exist and belong to same store
- ? `quantity`: Must be ? 1
- ? `unitPrice`: Must be ? 0
- ? Cannot add same product twice (use update quantity instead)

**Response 201**: Created OrderProductDto  
**Response 400**: Validation error or duplicate product  
**Response 404**: Order or product not found

**Example Success Response**:
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

**Example Error (Duplicate Product)**:
```json
{
  "message": "Product {productId} is already in order {orderId}. Use UpdateQuantity instead."
}
```

**Frontend Usage**:
```typescript
async function addProductToOrder(orderProduct: AddProductToOrderRequest) {
  const response = await api.post('/OrderProduct', orderProduct, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId,
      'Content-Type': 'application/json'
    }
  });
  return response.data;
}
```

---

##### PUT `/api/OrderProduct/{orderId}/product/{productId}` - Update Product Quantity

**Purpose**: Update the quantity of a product already in the order  
**Auth**: Required  
**Headers**: `X-Store-ID: {storeId}`  
**URL Params**: 
- `orderId` (guid)
- `productId` (guid)

**Request Body**:
```json
{
  "quantity": 5
}
```

**Validation Rules**:
- ? `quantity`: Must be ? 1
- ? If quantity = 0, use DELETE endpoint instead

**Response 200**: Updated OrderProductDto with new quantity and recalculated total  
**Response 404**: Product not found in order  
**Response 400**: Invalid quantity (e.g., quantity = 0)

**Example Success Response**:
```json
{
  "orderId": "order-guid",
  "productId": "product-guid",
  "productName": "Nike Air Max 2024",
  "quantity": 5,
  "unitPrice": 129.99,
  "totalPrice": 649.95
}
```

**Frontend Usage**:
```typescript
async function updateProductQuantity(orderId: string, productId: string, quantity: number) {
  const response = await api.put(
    `/OrderProduct/${orderId}/product/${productId}`,
    { quantity },
    {
      headers: {
        'Authorization': `Bearer ${token}`,
        'X-Store-ID': currentStoreId,
        'Content-Type': 'application/json'
      }
    }
  );
  return response.data;
}
```

---

##### DELETE `/api/OrderProduct/{orderId}/product/{productId}` - Remove Product from Order

**Purpose**: Remove a product line item from the order  
**Auth**: Required  
**Headers**: `X-Store-ID: {storeId}`  
**URL Params**: 
- `orderId` (guid)
- `productId` (guid)

**Response 204**: No content (success)  
**Response 404**: Product not found in order

**Frontend Usage**:
```typescript
async function removeProductFromOrder(orderId: string, productId: string) {
  await api.delete(`/OrderProduct/${orderId}/product/${productId}`, {
    headers: {
      'Authorization': `Bearer ${token}`,
      'X-Store-ID': currentStoreId
    }
  });
  // 204 No Content - no response body
}
```

---

#### Frontend Requirements

##### Order Details Page
```
???????????????????????????????????????????????????
? Order #12345                                   [Edit] ?
???????????????????????????????????????????????????
? Customer: John Doe                                   ?
? Status: Pending                      [Accept][Reject]?
? Created: Dec 19, 2024 10:30 AM                      ?
???????????????????????????????????????????????????
? Products                                             ?
? ???????????????????????????????????????????????????????     ?
? ? Product Name       ? Qty ? Price ? Total      ?   ?
? ???????????????????????????????????????????????????????   ?
? ? Nike Air Max 2024  ? 2   ? $129.99? $259.98 [X]?   ?
? ? Adidas Ultraboost  ? 1   ? $180.00? $180.00 [X]?   ?
? ???????????????????????????????????????????????????????   ?
? ? Order Total                       $439.98       ?   ?
? ???????????????????????????????????????????????????????   ?
?                                                      ?
? [+ Add Product]                                      ?
???????????????????????????????????????????????????
```

**Components Needed**:
1. **Product Table**:
   - Columns: Product Name, Quantity (editable), Unit Price (read-only), Total Price (calculated), Actions (remove button)
   - **Load**: `GET /api/OrderProduct/order/{orderId}`
   - **Inline Edit**: Quantity input with save/cancel buttons
   - **Remove Button**: Confirmation dialog ? DELETE request

2. **Add Product Button**:
   - Opens modal/dropdown
   - Product search/dropdown from `GET /api/products`
   - Quantity input (default: 1)
   - Unit price auto-filled (read-only)
   - **Submit**: `POST /api/OrderProduct`

3. **Order Total Display**:
   - **Calculated**: Sum of all `orderProduct.totalPrice`
   - **Updates automatically** after add/update/delete
   - **Backend calculates** authoritative total

4. **Quantity Edit**:
   - **Inline**: Click quantity ? Input field appears
   - **Save**: `PUT /api/OrderProduct/{orderId}/product/{productId}`
   - **Validation**: Must be ? 1
   - **Cancel**: Revert to original value

5. **Remove Product**:
   - **Button**: "X" icon or "Remove" button
   - **Confirmation**: "Remove {productName} from order?"
   - **Submit**: `DELETE /api/OrderProduct/{orderId}/product/{productId}`

##### Order Creation Form (Manual Orders)
```
???????????????????????????????????????????????????
? Create Order                                    [X] ?
???????????????????????????????????????????????????
? Customer: [Dropdown: Select customer]              ?
?                                                     ?
? Products:                                           ?
? ???????????????????????????????????????????       ?
? ? Product              ? Qty ? Unit Price ? Total??
? ???????????????????????????????????????????     ?
? ? Nike Air Max 2024    ? 2   ? $129.99    ? $259.98?
? ? Adidas Ultraboost    ? 1   ? $180.00    ? $180.00?
? ???????????????????????????????????????????     ?
? ? Order Total                           $439.98   ??
? ???????????????????????????????????????????       ?
?                                                     ?
? [+ Add Product]                                     ?
?                                                     ?
? [Cancel]                            [Create Order]  ?
???????????????????????????????????????????????????
```

**Workflow**:
1. User selects customer
2. User clicks "+ Add Product"
3. Product selection modal opens:
   - Search/filter products
   - Select product
   - Enter quantity
   - Preview: "{productName} × {quantity} = ${total}"
4. User clicks "Add"
5. Product added to table
6. User repeats for more products
7. Running total updates
8. User clicks "Create Order"
9. **Submit**: `POST /api/orders` with final total

**Note**: In this flow, OrderProducts are added **after** order creation. Alternative workflow:
1. User builds product list in UI (no API calls yet)
2. User clicks "Create Order"
3. Backend creates Order
4. Backend loops through products and creates OrderProducts
5. Backend calculates final total

**Recommended Approach**: Build product list in UI, submit all at once with order creation.

---

#### Critical Business Rules

##### 1. Price Lock-In Rule
**Rule**: `unitPrice` captured at order creation time is **immutable**.

**Why**: 
- Prevents retroactive price changes from affecting existing orders
- Reflects actual price customer agreed to pay
- Ensures order history integrity

**Example**:
```
1. Order created: Product A at $100, Quantity: 2, Total: $200
2. Product A price changed to $120
3. Existing order still shows $100 (locked-in price)
4. New orders will use $120 (current price)
```

**Frontend Implication**:
- When adding product to order, fetch current product price
- Display as "Unit Price (current)" to user
- Once added, unit price is locked and never changes

---

##### 2. Duplicate Prevention Rule
**Rule**: Cannot add same product twice to an order.

**Why**: 
- Prevents confusion with multiple line items for same product
- Enforces single line item per product (update quantity instead)

**Behavior**:
- **Attempt**: `POST /api/OrderProduct` with duplicate productId
- **Response**: `400 Bad Request` with message: "Product already in order. Use UpdateQuantity instead."

**Frontend Implication**:
- Before adding product, check if already in order
- If exists, show message: "This product is already in the order. Increase quantity instead?"
- Redirect to quantity update

**Implementation**:
```typescript
function canAddProduct(orderId: string, productId: string, orderProducts: OrderProduct[]): boolean {
  return !orderProducts.some(op => op.productId === productId);
}

async function addOrUpdateProduct(orderId: string, productId: string, quantity: number) {
  const existing = orderProducts.find(op => op.productId === productId);
  
  if (existing) {
    // Update existing
    await updateProductQuantity(orderId, productId, existing.quantity + quantity);
  } else {
    // Add new
    await addProductToOrder({ orderId, productId, quantity, unitPrice });
  }
}
```

---

##### 3. Automatic Total Calculation Rule
**Rule**: Order `totalPrice` = sum of all OrderProduct `totalPrice` values.

**Why**:
- Backend calculates authoritative total
- Frontend displays but doesn't submit total
- Prevents tampering with order total

**Calculation**:
```
OrderProduct 1: Quantity: 2, UnitPrice: $129.99 ? Total: $259.98
OrderProduct 2: Quantity: 1, UnitPrice: $180.00 ? Total: $180.00
-----------------------------------------------------------
Order Total: $439.98
```

**Frontend Implication**:
- Display calculated total for user preview
- **Do not** include total in order creation request
- Backend recalculates from OrderProducts

**API Behavior**:
```typescript
// ? CORRECT: Backend calculates total
POST /api/orders
{
  customerId: "customer-guid"
  // No totalPrice field - backend calculates
}

// ? WRONG: Frontend sends total
POST /api/orders
{
  customerId: "customer-guid",
  totalPrice: 439.98 // Ignored by backend
}
```

---

##### 4. Quantity Validation Rule
**Rule**: Quantity must always be ? 1.

**Why**:
- Quantity = 0 is invalid (use DELETE instead)
- Negative quantities are nonsensical

**Validation**:
- **Frontend**: Disable submit if quantity < 1
- **Backend**: Returns `400 Bad Request` if quantity ? 0

**Frontend Implementation**:
```tsx
<input
  type="number"
  min="1"
  value={quantity}
  onChange={(e) => setQuantity(Math.max(1, parseInt(e.target.value)))}
/>
```

---

##### 5. Cascade Delete Rule
**Rule**: Deleting an order ? All OrderProducts deleted.

**Why**:
- OrderProducts cannot exist without parent order
- Database cascade delete enforced

**Frontend Implication**:
- When deleting order, no need to manually delete OrderProducts
- Backend handles cascade automatically

---

##### 6. Product Reference Integrity Rule
**Rule**: OrderProduct links to Product, but keeps `productName` for history.

**Why**:
- If product is deleted, OrderProduct preserves product name
- ProductId may become null (soft reference)
- Order history remains intact

**Behavior**:
```
1. Order created with "Nike Air Max 2024" (productId: abc-123)
2. Product "Nike Air Max 2024" deleted by admin
3. OrderProduct still shows "Nike Air Max 2024" in order history
4. ProductId may become null or remain (depends on soft delete implementation)
```

**Frontend Implication**:
- Always display `productName` (not product.name)
- If productId is null, show "(Product Deleted)" badge
- Order history always readable

---

#### Use Cases

##### Use Case 1: Build Order with Multiple Products

**Scenario**: Admin creates order for customer with 3 products.

**Steps**:
1. Admin: Create order ? `POST /api/orders` (customerId: "customer-guid")
   - Response: `{ id: "order-guid", status: "Pending", totalPrice: 0 }`
2. Admin: Add Product A (qty: 2) ? `POST /api/OrderProduct`
   - Request: `{ orderId: "order-guid", productId: "product-A", quantity: 2, unitPrice: 129.99 }`
   - Response: `{ ..., totalPrice: 259.98 }`
3. Admin: Add Product B (qty: 1) ? `POST /api/OrderProduct`
   - Request: `{ orderId: "order-guid", productId: "product-B", quantity: 1, unitPrice: 180.00 }`
   - Response: `{ ..., totalPrice: 180.00 }`
4. Admin: Add Product C (qty: 5) ? `POST /api/OrderProduct`
   - Request: `{ orderId: "order-guid", productId: "product-C", quantity: 5, unitPrice: 45.00 }`
   - Response: `{ ..., totalPrice: 225.00 }`
5. System: Order total = 259.98 + 180.00 + 225.00 = **$664.98**

**Result**:
- Order created with 3 line items
- Total price auto-calculated by backend
- Admin views order details with full breakdown

---

##### Use Case 2: Customer Changes Mind (Edit Order)

**Scenario**: Customer calls to change quantity after order placed.

**Steps**:
1. Admin: View order ? `GET /api/OrderProduct/order/{orderId}`
   - Response: Array of 3 products
2. Customer: "Change Product A from 2 to 3"
3. Admin: Update quantity ? `PUT /api/OrderProduct/{orderId}/product/{productA}`
   - Request: `{ quantity: 3 }`
   - Response: `{ ..., quantity: 3, totalPrice: 389.97 }`
4. System: Order total recalculates = 389.97 + 180.00 + 225.00 = **$794.97**

**Result**:
- Quantity updated
- Line total recalculated
- Order total recalculated
- Customer charged correct amount

---

##### Use Case 3: Remove Unwanted Item

**Scenario**: Customer decides not to buy Product C.

**Steps**:
1. Admin: View order ? `GET /api/OrderProduct/order/{orderId}`
2. Customer: "Remove Product C from order"
3. Admin: Click "Remove" ? Confirmation dialog
4. Admin: Confirm ? `DELETE /api/OrderProduct/{orderId}/product/{productC}`
   - Response: `204 No Content`
5. System: Order total recalculates = 389.97 + 180.00 = **$569.97**

**Result**:
- Product C removed from order
- Order total updated
- Customer charged for 2 products only

---

##### Use Case 4: Price Change After Order

**Scenario**: Admin changes product price, existing orders unaffected.

**Steps**:
1. **Before**: Order created with Product A at $100, Quantity: 2, Total: $200
2. **Admin Action**: Change Product A price to $120
3. **Existing Order**: Still shows $100 (locked-in price)
   - `GET /api/OrderProduct/order/{orderId}` ? `{ ..., unitPrice: 100.00, totalPrice: 200.00 }`
4. **New Order**: Uses $120 (current price)
   - `POST /api/OrderProduct` ? `{ ..., unitPrice: 120.00 }`

**Result**:
- Historical data integrity preserved
- Existing orders not affected by price changes
- New orders use current pricing

---

#### Error Scenarios

##### Scenario 1: Duplicate Product

**Request**:
```http
POST /api/OrderProduct
{
  "orderId": "order-guid",
  "productId": "product-A",
  "quantity": 2,
  "unitPrice": 129.99
}
```

**Response 400**:
```json
{
  "message": "Product {productId} is already in order {orderId}. Use UpdateQuantity instead."
}
```

**Frontend Handling**:
```typescript
try {
  await addProductToOrder(orderProduct);
} catch (error) {
  if (error.response?.status === 400 && error.response.data.message.includes('already in order')) {
    showToast('Product already in order. Updating quantity instead...', 'info');
    await updateProductQuantity(orderId, productId, currentQty + newQty);
  }
}
```

---

##### Scenario 2: Invalid Quantity (Zero)

**Request**:
```http
PUT /api/OrderProduct/{orderId}/product/{productId}
{
  "quantity": 0
}
```

**Response 400**:
```json
{
  "message": "Quantity must be greater than 0."
}
```

**Frontend Handling**:
- **Validation**: Disable submit if quantity ? 0
- **Alternative**: If user sets quantity to 0, show: "Remove this product instead?"

---

##### Scenario 3: Product Not in Order

**Request**:
```http
PUT /api/OrderProduct/{orderId}/product/{productId}
```

**Response 404**:
```json
{
  "message": "Product {productId} not found in order {orderId}."
}
```

**Frontend Handling**:
```typescript
try {
  await updateProductQuantity(orderId, productId, quantity);
} catch (error) {
  if (error.response?.status === 404) {
    showToast('Product not found in order. Refreshing...', 'warning');
    await refreshOrderProducts(orderId);
  }
}
```

---

##### Scenario 4: Order Not Found

**Request**:
```http
POST /api/OrderProduct
{
  "orderId": "invalid-guid",
  "productId": "product-A",
  "quantity": 1,
  "unitPrice": 129.99
}
```

**Response 404**:
```json
{
  "message": "Order with ID invalid-guid not found."
}
```

**Frontend Handling**:
- Unlikely scenario (order deleted while adding products)
- Redirect to orders list with error message

---

##### Scenario 5: Product Not Found

**Request**:
```http
POST /api/OrderProduct
{
  "orderId": "order-guid",
  "productId": "invalid-product-guid",
  "quantity": 1,
  "unitPrice": 129.99
}
```

**Response 404**:
```json
{
  "message": "Product with ID invalid-product-guid not found."
}
```

**Frontend Handling**:
- Refresh product list
- Show error: "Product not found. It may have been deleted."

---

#### Complete Frontend Implementation Example

```typescript
// ============ OrderProduct Service ============

interface OrderProduct {
  orderId: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number; // Read-only, calculated by backend
}

interface AddProductToOrderRequest {
  orderId: string;
  productId: string;
  quantity: number;
  unitPrice: number;
}

interface UpdateOrderProductQuantityRequest {
  quantity: number;
}

class OrderProductService {
  private apiUrl = '/api/OrderProduct'; // Current route

  async getOrderProducts(orderId: string): Promise<OrderProduct[]> {
    const response = await api.get(`${this.apiUrl}/order/${orderId}`, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'X-Store-ID': getCurrentStoreId()
      }
    });
    return response.data;
  }

  async addProductToOrder(request: AddProductToOrderRequest): Promise<OrderProduct> {
    const response = await api.post(this.apiUrl, request, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'X-Store-ID': getCurrentStoreId(),
        'Content-Type': 'application/json'
      }
    });
    return response.data;
  }

  async updateProductQuantity(
    orderId: string,
    productId: string,
    quantity: number
  ): Promise<OrderProduct> {
    const response = await api.put(
      `${this.apiUrl}/${orderId}/product/${productId}`,
      { quantity },
      {
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'X-Store-ID': getCurrentStoreId(),
          'Content-Type': 'application/json'
        }
      }
    );
    return response.data;
  }

  async removeProductFromOrder(orderId: string, productId: string): Promise<void> {
    await api.delete(`${this.apiUrl}/${orderId}/product/${productId}`, {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'X-Store-ID': getCurrentStoreId()
      }
    });
  }

  calculateOrderTotal(orderProducts: OrderProduct[]): number {
    return orderProducts.reduce((sum, op) => sum + op.totalPrice, 0);
  }
}

// ============ React Component Example ============

function OrderDetailsPage({ orderId }: { orderId: string }) {
  const [orderProducts, setOrderProducts] = useState<OrderProduct[]>([]);
  const [loading, setLoading] = useState(true);
  const orderProductService = new OrderProductService();

  useEffect(() => {
    loadOrderProducts();
  }, [orderId]);

  async function loadOrderProducts() {
    setLoading(true);
    try {
      const products = await orderProductService.getOrderProducts(orderId);
      setOrderProducts(products);
    } catch (error) {
      showToast('Failed to load order products', 'error');
    } finally {
      setLoading(false);
    }
  }

  async function handleQuantityUpdate(productId: string, newQuantity: number) {
    try {
      const updated = await orderProductService.updateProductQuantity(
        orderId,
        productId,
        newQuantity
      );
      setOrderProducts(prev =>
        prev.map(op => (op.productId === productId ? updated : op))
      );
      showToast('Quantity updated', 'success');
    } catch (error) {
      showToast('Failed to update quantity', 'error');
    }
  }

  async function handleRemoveProduct(productId: string) {
    if (!confirm('Remove this product from order?')) return;

    try {
      await orderProductService.removeProductFromOrder(orderId, productId);
      setOrderProducts(prev => prev.filter(op => op.productId !== productId));
      showToast('Product removed', 'success');
    } catch (error) {
      showToast('Failed to remove product', 'error');
    }
  }

  const orderTotal = orderProductService.calculateOrderTotal(orderProducts);

  return (
    <div>
      <h2>Order Products</h2>
      <table>
        <thead>
          <tr>
            <th>Product Name</th>
            <th>Quantity</th>
            <th>Unit Price</th>
            <th>Total</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {orderProducts.map(op => (
            <tr key={op.productId}>
              <td>{op.productName}</td>
              <td>
                <input
                  type="number"
                  min="1"
                  value={op.quantity}
                  onChange={(e) => handleQuantityUpdate(op.productId, parseInt(e.target.value))}
                />
              </td>
              <td>${op.unitPrice.toFixed(2)}</td>
              <td>${op.totalPrice.toFixed(2)}</td>
              <td>
                <button onClick={() => handleRemoveProduct(op.productId)}>
                  Remove
                </button>
              </td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr>
            <td colSpan={3}><strong>Order Total:</strong></td>
            <td><strong>${orderTotal.toFixed(2)}</strong></td>
            <td></td>
          </tr>
        </tfoot>
      </table>
      <button onClick={() => {/* Open add product modal */}}>+ Add Product</button>
    </div>
  );
}
```

---

### 5. Marketing Campaigns

#### Business Scenario
Store owners create marketing campaigns to promote products across social media platforms. **Campaigns follow a strict multi-step wizard flow**.

#### Campaign Stages (CANONICAL)
```
Draft ? InReview ? Scheduled ? Ready ? Published
```

**Stage Descriptions**:
- **Draft**: Initial creation, posts not scheduled
- **InReview**: Under review (future - not used yet)
- **Scheduled**: Posts scheduled, awaiting publish time
- **Ready**: Campaign activated, posts publishing immediately
- **Published**: All posts published, campaign complete

#### Entity: Campaign
```json
{
  "id": "guid",
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
  "createdByUserId": "user-guid",
  "createdByUserName": "John Doe",
  "storeId": "store-guid",
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-05-15T10:00:00Z"
}
```

#### Campaign Workflow (3-Step Wizard)

**See detailed flow above in "Campaign Creation Flow (EXPLICIT SEQUENCING)"**

Summary:
1. **Step 1**: Create campaign (Draft state)
2. **Step 2**: Add multiple posts (one at a time)
3. **Step 3**: Publish now OR schedule

#### Frontend Requirements
- **Campaign List Page**
  - **Load**: `GET /api/campaigns` (STORE-SCOPED)
  - **Columns**: Name, Stage, Start Date, End Date, Actions
- **Campaign Creation Wizard**:
  - **Step 1 Page**: `/campaigns/create`
    - Campaign details form
    - **Submit**: `POST /api/campaigns` (stage: "Draft")
  - **Step 2 Page**: `/campaigns/{campaignId}/posts`
    - Post list + "Add Post" button
    - **Add Post**: `POST /api/campaign-posts`
  - **Step 3 Page**: `/campaigns/{campaignId}/publish`
    - Publish now OR schedule options
    - **Update**: `PUT /api/campaigns/{campaignId}`
- **Campaign Dashboard** (analytics) - future
- **Campaign Stage Indicator**
  - Color-coded badge (Draft: Gray, Scheduled: Blue, Published: Green)

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

#### Post Creation Flow (Part of Campaign Wizard Step 2)

**See detailed flow in "Campaign Creation Flow - Step 2"**

Key Points:
1. **Load connected platforms**: `GET /api/social-platforms/connected` - **NOT YET IMPLEMENTED**
   - **Temporary Workaround**: Hardcode Facebook as only available platform
   - **Future**: Use API response when ready
2. Show checkboxes for platforms where `isConnected = true`
   - **Current**: Only Facebook checkbox (hardcoded)
   - **Future**: Dynamic checkboxes from API
3. User selects platforms + enters caption + uploads image
4. Submit: `POST /api/campaign-posts` with `platformIds` array
5. Backend creates `CampaignPost` + `CampaignPostPlatform` records

**Platform Selection (Temporary Implementation)**:
```typescript
// TEMPORARY: Until GET /api/social-platforms/connected is implemented
const connectedPlatforms = [
  {
    id: 'facebook-platform-guid', // Get this from backend or hardcode
    platformName: 'Facebook',
    isConnected: true
  }
];

// FUTURE: Replace with API call
async function loadConnectedPlatforms() {
  const response = await api.get('/social-platforms/connected');
  return response.data; // Returns array of connected platforms
}
```

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

#### Publishing Workflow (Automatic - No Frontend Action)
1. **Hangfire Background Job** runs every 1 minute
2. Finds posts with `scheduledAt <= now` and `publishStatus = Pending`
3. For each post, publish to all selected platforms
4. Facebook Publisher uses Graph API
5. Updates `publishStatus` to "Published" or "Failed"
6. Stores `externalPostId` from platform
7. Records any errors

#### Publish Statuses
- **Pending**: Not yet published (awaiting scheduled time)
- **Publishing**: Currently being published (transient state)
- **Published**: Successfully published to all platforms
- **Failed**: Publishing failed (see errorMessage)

#### Frontend Requirements
- **Post Creation Form** (modal within campaign wizard)
  - Campaign dropdown (if standalone page)
  - Caption textarea
  - Image URL input
  - **Platform checkboxes**:
    - **TEMPORARY**: Show only Facebook (hardcoded)
    - **FUTURE**: Load from `GET /api/social-platforms/connected`
    - **Render only**: platforms where `isConnected = true`
  - Scheduled time picker
  - **Submit**: `POST /api/campaign-posts`
- **Post List Page** (per campaign)
  - **Load**: `GET /api/campaign-posts` (filter by campaign client-side)
  - **Columns**: Caption, Scheduled At, Platforms, Status
- **Post Status Indicator**
  - Pending: Yellow, Published: Green, Failed: Red
- **Retry Failed Posts Button** - future
- **View on Platform Link** (using externalPostId) - future

---

### 7. Social Media Platform Management

#### Business Scenario
Store owner connects their social media accounts to enable automated posting.

#### Supported Platforms
- ? **Facebook** (OAuth integration complete)
- ?? **Instagram** (planned - not available)
- ?? **TikTok** (planned - not available)
- ?? **YouTube** (planned - not available)

**Frontend Note**: Until `GET /api/social-platforms/connected` is implemented, assume only Facebook is available.

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
2. Facebook OAuth modal opens (Facebook SDK)
3. User authorizes app
4. Facebook returns access token + page ID
5. Frontend sends to `/api/social-platforms/facebook/connect`
   - **API**: `POST /api/social-platforms/facebook/connect` (STORE-SCOPED)
6. Backend validates token with Facebook
7. Backend stores connection
8. Platform appears as "Connected" in UI

#### Platform Uses
- **Campaign Post Publishing**: Auto-post to connected accounts (Hangfire job)
- **Chatbot Order Resolution**: Facebook Page ID ? Store ID mapping (backend only)

#### Frontend Requirements
- **Platform List Page**
  - **Load**: `GET /api/social-platforms` (STORE-SCOPED) - *API needs implementation*
  - **Alternative**: Use `GET /api/social-platforms/connected` for now - *NOT YET IMPLEMENTED*
  - **Temporary**: Show only Facebook connection status
  - **Columns**: Platform, Page Name, Status, Actions
- **Connect Platform Buttons**
  - **Facebook**: `POST /api/social-platforms/facebook/connect`
  - **OAuth Modal**: Use Facebook SDK
- **Platform Status Indicators**
  - Connected: Green, Disconnected: Gray
- **Disconnect/Reconnect Actions**
  - **Disconnect**: `PUT /api/social-platforms/{connectionId}/disconnect`
- ** Platform Dropdown** (for available platforms)
  - **API**: `GET /api/social-platforms/available-platforms` (GLOBAL)
  - **Returns**: All platform types (Facebook, Instagram, TikTok, YouTube)
  - **Note**: Only Facebook is functional currently

**API for Platform Checkboxes in Post Creation**:
```javascript
// Get connected platforms for current store
GET /api/social-platforms/connected // ?? NOT YET IMPLEMENTED

// TEMPORARY WORKAROUND: Hardcode Facebook
const platforms = [{ id: 'facebook-guid', platformName: 'Facebook', isConnected: true }];

// FUTURE: Use API when ready
const response = await api.get('/social-platforms/connected');
const platforms = response.data;
```

---

### 8. Team Collaboration *(Future - Placeholder)*

**Status**: UI placeholders only, no functional backend yet

#### Business Scenario
Store owner invites team members to help manage the store with different permission levels. **Currently, users can see teams they belong to on the homepage, but cannot invite others or manage permissions.**

#### Team Roles (Future)
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

#### Frontend Requirements (Placeholder)
- **Homepage Team List** (read-only)
  - **Load**: `GET /api/teams/my` (GLOBAL)
  - **Display**: Team name + parent store name
  - **Click**: Does nothing (disabled state)
  - **Tooltip**: "Team collaboration coming soon"
- **Team List Page** (placeholder UI)
  - Disabled cards, no actions
- **Team Creation Modal** (disabled button)
- **Member List** (placeholder, no data)
- **Invite Member Form** (disabled, no submission)

**Copilot MUST Generate**:
- ? UI-only placeholders (disabled buttons, grayed-out cards)
- ? Tooltips: "Feature coming soon"
- ? Homepage team list (read-only, from `GET /api/teams/my`)

**Copilot MUST NOT Generate**:
- ? Team invitation forms (functional)
- ? Role assignment dropdowns (functional)
- ? Permission management UI
- ? API calls to `/api/teams/{id}/members` or similar endpoints

**DO NOT** build functional team features yet - backend incomplete.

---

### 9. Chatbot Integration (n8n) - CORRECTED

...existing code...

---

### 10. Automation & Background Jobs

...existing code...

---

## ?? Authentication & Authorization

...existing code...

---

## ?? Data Relationships

...existing code...

---

## ?? Frontend Application Structure

### Recommended Pages

```
/
??? /auth
?   ??? /login ? POST /api/auth/login
?   ??? /register ? POST /api/auth/register
?
??? / or /home (Homepage - Store/Team Selection)
?   ??? Store grid ? GET /api/stores/my
?   ??? Team list ? GET /api/teams/my (placeholder)
?   ??? "+ New Store" card ? Opens modal
?   ??? Empty states ? "Start a Store" + "Join a Team"
?
??? /dashboard (Store-scoped overview - after store selection)
?   ??? Pending orders widget ? GET /api/orders?status=Pending
?   ??? Revenue chart (future)
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
?   ??? /create ? WIZARD STEP 1 (POST /api/campaigns)
?   ??? /:id/posts ? WIZARD STEP 2 (POST /api/campaign-posts)
?   ??? /:id/publish ? WIZARD STEP 3 (PUT /api/campaigns/{id})
?   ??? /:id/details ? GET /api/campaigns/{id}
?
??? /social-platforms
?   ??? /list ? GET /api/social-platforms (needs implementation)
?   ??? /connect/facebook ? POST /api/social-platforms/facebook/connect
?
??? /teams (placeholder - not functional)
?   ??? /list ? Placeholder UI (disabled)
?   ??? /create ? Disabled button
?   ??? /:id/members ? Placeholder (no data)
?
??? /automation (future)
?   ??? /tasks ? GET /api/automation-tasks
?
??? /settings
    ??? /profile ? GET /api/users/me, PUT /api/users/{id}
    ??? /store ? GET /api/stores/{id}, PUT /api/stores/{id}
```

---

## ?? Complete API Endpoints Reference

...existing code...

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

**Current Status**: **NOT YET IMPLEMENTED**

**Frontend Workaround**: 
- Hardcode Facebook as only available platform
- Use placeholder platform ID or query by known ID

**Future Implementation**: 
- When API is ready, replace hardcoded platform with API response
- Enable multi-platform UI (Instagram, TikTok, YouTube)

---

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

**Frontend Workaround**: Show only Facebook connection status for now

---

## ?? User Stories & Scenarios

### Store Owner Journey

**Day 1: Setup**
1. Register account ? JWT token stored
2. **Redirected to homepage** ? No stores yet
3. See "Start a Store Now" empty state
4. Click "Create Your First Store" ? Modal opens
5. Create first store "Fashion Hub" ? Store ID stored
6. Click store card ? Redirected to `/dashboard`
7. Add 10 products ? Auto-embedding to n8n
8. Connect Facebook page ? OAuth flow
9. Create first campaign (Draft) ? Add 3 posts ? Schedule

**Day 2: First Sales**
1. Customer orders via Facebook Messenger (n8n handles)
2. n8n sends order to `/api/orders/chatbot`
3. Order appears in pending orders widget (frontend polls)
4. Admin reviews order ? Clicks "Accept"
5. Backend notifies n8n ? n8n notifies customer

**Day 3: Marketing**
1. Create new campaign "Summer Sale" (Draft)
2. Assign featured product
3. Add 5 scheduled posts (**only Facebook platform available**)
4. Click "Publish Now" ? Posts publish immediately
5. Hangfire job processes posts
6. Campaign stage ? "Published"

**Day 7: Scaling**
1. Homepage shows 3 owned stores
2. Click "+ New Store" ? Create "Tech Store"
3. Switch between stores via header dropdown
4. Add 50 more products to each store
5. Create 3 more campaigns per store
6. *(Future)* Connect Instagram when available
7. *(Placeholder)* See teams on homepage (disabled)

### Customer Journey (Chatbot)

**Ordering via Messenger** (n8n perspective):
1. Customer: "I want to order Nike shoes"
2. n8n Bot: "What size?"
3. Customer: "Size 10, quantity 2"
4. n8n Bot: "What's your phone and address?"
5. Customer provides details
6. n8n: Sends order to `/api/orders/chatbot`
7. n8n Bot: "Order placed! Order ID: XYZ123. Total: $259.98"
8. *[Admin accepts via frontend]*
9. n8n Bot: "Your order is confirmed! Tracking: ..."

---

## ?? Business Metrics & Analytics (Future)

### Key Performance Indicators (KPIs)

**Sales Metrics**:
- Total orders (by status)
- Revenue (today, week, month)
- Average order value
- Top-selling products

**Marketing Metrics**:
- Active campaigns
- Posts scheduled/published
- Social media reach (future)
- Campaign ROI (future)

**Operational Metrics**:
- Pending orders count
- Order processing time
- Product catalog size
- Customer acquisition rate

### Dashboard Widgets (Future)

1. **Revenue Chart** (line chart, 30 days)
2. **Orders by Status** (pie chart)
3. **Top Products** (bar chart)
4. **Recent Orders** (table)
5. **Pending Orders Alert** (badge - **IMPLEMENTED**)

---

## ?? Future Enhancements

### Phase 2 Features
- [ ] Mobile app (React Native)
- [ ] Customer self-service portal
- [ ] Inventory management
- [ ] Multi-currency support
- [ ] Shipping integrations
- [ ] Payment gateway integration
- [ ] **Team collaboration** (complete implementation)

### Phase 3 Features
- [ ] Advanced analytics dashboard
- [ ] AI-powered product recommendations
- [ ] Email marketing automation
- [ ] SMS notifications
- [ ] WhatsApp integration
- [ ] Multi-language support

---

## ?? Integration Partners

### Current Integrations
- **n8n**: Workflow automation (chatbot conversation handling, product embedding)
- **Facebook**: OAuth, Graph API (posts, page management)

### Planned Integrations
- **Instagram**: Business API
- **TikTok**: For Business
- **YouTube**: Data API
- **Stripe**: Payment processing
- **SendGrid**: Email delivery
- **Twilio**: SMS notifications

---

## ?? Technical Stack Recommendations

### Frontend Technology Stack
- **Framework**: React 18+ or Next.js 14+
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: React Context API + React Query (TanStack Query)
- **Forms**: React Hook Form
- **Charts**: Recharts or Chart.js
- **Date Pickers**: react-datepicker
- **Tables**: TanStack Table (react-table)
- **HTTP Client**: Axios or Fetch API
- **Routing**: React Router v6
- **Internationalization**: react-i18next or next-i18next (for Arabic/English)
- **RTL Support**: CSS logical properties, direction context

### Component Libraries (Optional)
- **Ant Design** - Enterprise UI components (supports RTL)
- **Material-UI** - Google Material Design (supports RTL)
- **Chakra UI** - Accessible component library (supports RTL)
- **Shadcn/ui** - Unstyled, customizable components

---

## ?? Design System Guidelines

### Color Palette
- **Primary**: Blue (#2563eb) - Actions, links
- **Success**: Green (#16a34a) - Confirmations
- **Warning**: Orange (#f59e0b) - Alerts
- **Danger**: Red (#dc2626) - Errors
- **Info**: Cyan (#06b6d4) - Information

### Typography
- **Headers**: Inter or Poppins (bold)
- **Body**: Inter or System UI
- **Monospace**: Fira Code (for IDs, codes)
- **Arabic Font**: Cairo, Tajawal, or Noto Sans Arabic

### Status Colors
- **Pending**: Yellow (#fbbf24)
- **Accepted**: Blue (#3b82f6)
- **Shipped**: Purple (#8b5cf6)
- **Delivered**: Green (#10b981)
- **Rejected**: Red (#ef4444)
- **Cancelled**: Gray (#6b7280)

### Bilingual Support (Arabic/English)
- **Language Switcher**: Dropdown in navbar (?? EN ? / ?? AR ?)
- **RTL Layout**: Entire UI flips for Arabic (flexbox `row-reverse`, text alignment)
- **LTR Layout**: Default for English
- **Icons**: Remain logically positioned (e.g., chevrons flip direction)
- **Text Alignment**: Left for English, right for Arabic
- **Date Formats**: Locale-aware (e.g., "Dec 19, 2024" vs "?? ?????? ????")

### Design Principles
- **Professional**: Clean, modern, business-appropriate
- **Alive**: Smooth animations, hover effects, transitions
- **Elegant**: Good spacing, subtle shadows, balanced colors
- **Responsive**: Mobile-first, tablet, desktop breakpoints
- **Accessible**: WCAG 2.1 AA compliance, keyboard navigation

---

## ?? Glossary

**Store**: A business entity that owns products, customers, and orders  
**PSID**: Page-Scoped ID (Facebook's unique identifier for users per page)  
**Campaign**: Marketing initiative to promote products across social media  
**Post**: Social media content scheduled for publishing  
**Platform**: Social media channel (Facebook, Instagram, etc.)  
**Team**: Group of users collaborating on a store *(placeholder - not functional)*  
**Chatbot Order**: Order received via Facebook Messenger (handled by n8n, sent to backend as finalized)  
**Embedding**: Vector representation of products for AI search (automatic, via n8n)  
**n8n**: External workflow automation platform that handles Facebook Messenger conversations  
**Hangfire**: Background job scheduler (publishes posts, sends notifications)  
**Homepage**: Landing page after login where user selects store or creates new one  
**Store-Scoped**: API endpoint that requires `X-Store-ID` header  
**RTL**: Right-to-Left (Arabic language layout direction)  
**LTR**: Left-to-Right (English language layout direction)

---

## ?? FINAL CHECKLIST FOR FRONTEND GENERATION

### ? Must Understand
- [ ] Backend **DOES NOT** receive raw Facebook messages
- [ ] n8n handles all chatbot conversations
- [ ] Backend only receives **finalized orders** from n8n
- [ ] Store resolution (PageID ? StoreID) is **backend internal**
- [ ] Frontend never sends StoreID with chatbot orders
- [ ] **Frontend NEVER calls `/api/orders/chatbot`** (n8n exclusive endpoint)
- [ ] Order statuses are **EXACT strings** (Pending, Accepted, etc.)
- [ ] Campaign stages are **EXACT strings** (Draft, Scheduled, etc.)
- [ ] Campaign creation is a **3-step wizard**, not a single form
- [ ] Team collaboration is a **placeholder** (do not build functional UI)
- [ ] **Only Facebook platform is available** until API endpoint is ready
- [ ] **Order source detection**: Prefer `orderSource` field if exists, else infer from PSID
- [ ] **Homepage is the landing page** after login (store/team selection)

### ? Must Implement
- [ ] **Homepage** with store grid + team list (placeholder)
- [ ] **Empty state** for no stores/teams ("Start a Store", "Join a Team")
- [ ] **"+ New Store" card** in store grid (same size as stores)
- [ ] Store selector dropdown (always visible in header after store selection)
- [ ] HTTP interceptor (auto-add X-Store-ID header)
- [ ] Campaign creation wizard (3 steps)
- [ ] Platform checkboxes in post creation (**hardcode Facebook only for now**)
- [ ] Pending orders dashboard widget (poll every 30 seconds)
- [ ] Accept/Reject buttons (enabled only for pending orders)
- [ ] Order status badges (color-coded)
- [ ] Campaign stage badges (color-coded)
- [ ] **Chatbot order badge** (show if `isChatbotOrder()` returns true)
- [ ] **Language switcher** (English/Arabic dropdown in navbar)
- [ ] **RTL support** (layout flips for Arabic)

### ? Must NOT Implement
- [ ] Chat UI components (no message handling)
- [ ] Websocket connections for chat
- [ ] Facebook message receiving logic
- [ ] Store resolution logic (backend handles)
- [ ] Functional team invitation system (placeholder only)
- [ ] **Multi-platform UI for Instagram/TikTok/YouTube** (not ready yet)
- [ ] **Functional team management** (disabled buttons only)
- [ ] **Chatbot order creation forms** (frontend never creates chatbot orders)
- [ ] **Services/hooks calling `/api/orders/chatbot`** (n8n exclusive endpoint)
- [ ] **Any UI for submitting to chatbot endpoint** (security risk)

### ? Design Requirements
- [ ] **Professional & Alive**: Modern, smooth animations
- [ ] **Elegant**: Clean, spacious, subtle shadows
- [ ] **Bilingual**: Full English/Arabic support
- [ ] **RTL/LTR**: Proper layout flipping
- [ ] **Responsive**: Mobile, tablet, desktop
- [ ] **Accessible**: Keyboard navigation, ARIA labels

---

**End of Business Scenario Documentation (Corrected for Frontend Generation - Final)**

This document is **fully optimized for AI-powered frontend code generation** with all critical clarifications, homepage flow, bilingual support, and temporary workarounds for missing APIs.
