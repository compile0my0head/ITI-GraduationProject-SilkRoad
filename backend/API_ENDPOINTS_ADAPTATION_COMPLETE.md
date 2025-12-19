# API Endpoints Adaptation - Complete Summary

## ?? Objective

Adapt all API endpoints to match the business requirements:
- Flat REST URLs (no nested routes)
- Plural naming for routes
- Clear distinction between GLOBAL and STORE-SCOPED endpoints
- Proper parameter naming (consistent use of specific IDs like `storeId`, `teamId`, etc.)
- Correct GUID usage for all IDs

---

## ? Changes Applied

### 1. Authentication Endpoints (GLOBAL - NO X-Store-ID)

**Route:** `/api/auth`

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| POST | `/api/auth/login` | User login | ? Already correct |
| POST | `/api/auth/register` | User registration | ? Renamed from `signup` |
| POST | `/api/auth/logout` | User logout | ? Added new endpoint |

**Files Modified:**
- `Presentation/Controllers/AuthController.cs`

---

### 2. User Endpoints (GLOBAL - NO X-Store-ID)

**Route:** `/api/users`

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/users/me` | Get current user profile | ? Added new endpoint |
| GET | `/api/users/{id}` | Get user by ID | ? Already correct |
| GET | `/api/users/by-email/{email}` | Get user by email | ? Already correct |
| PUT | `/api/users/{id}` | Update user | ? Already correct |
| DELETE | `/api/users/{id}` | Delete user | ? Already correct |

**Files Modified:**
- `Presentation/Controllers/UsersController.cs` - Added `GET /me` endpoint, injected `ICurrentUserService`

---

### 3. Store Endpoints (GLOBAL - NO X-Store-ID)

**Route:** `/api/stores` (changed from `/api/[controller]`)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/stores/my` | Get user's accessible stores | ? Renamed from `my-stores` |
| GET | `/api/stores/{storeId}` | Get store by ID | ? Changed param from `id` to `storeId` |
| POST | `/api/stores` | Create new store | ? Already correct |
| PUT | `/api/stores/{storeId}` | Update store | ? Changed param from `id` to `storeId` |
| DELETE | `/api/stores/{storeId}` | Delete store | ? Changed param from `id` to `storeId` |

**Files Modified:**
- `Presentation/Controllers/StoreController.cs` - Changed route to `/api/stores`, renamed endpoints, updated parameter names

---

### 4. Team Endpoints (MIXED: GLOBAL + STORE-SCOPED)

**Route:** `/api/teams`

#### GLOBAL Endpoints (NO X-Store-ID)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/teams/my` | Get user's teams across all stores | ? Added new endpoint |
| GET | `/api/teams/{teamId}` | Get team by ID | ? Changed param from `id` to `teamId` |

#### STORE-SCOPED Endpoints (X-Store-ID REQUIRED)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/teams` | Get all teams in current store | ? Already correct |
| POST | `/api/teams` | Create team in current store | ? Already correct |
| PUT | `/api/teams/{teamId}` | Update team | ? Changed param from `id` to `teamId` |
| DELETE | `/api/teams/{teamId}` | Delete team | ? Changed param from `id` to `teamId` |
| GET | `/api/teams/{teamId}/members` | Get team members | ? Changed param from `id` to `teamId` |
| POST | `/api/teams/{teamId}/members` | Add team member | ? Changed param from `id` to `teamId` |
| DELETE | `/api/teams/{teamId}/members/{userId}` | Remove team member | ? Changed params |

**Files Modified:**
- `Presentation/Controllers/TeamController.cs` - Added `GET /my` endpoint, updated parameter names
- `Application/Common/Interfaces/ITeamService.cs` - Added `GetMyTeamsAsync` method, removed `GetByStoreIdAsync`
- `Application/Services/TeamService.cs` - Implemented `GetMyTeamsAsync`, injected `ICurrentUserService` and `IStoreAuthorizationService`

---

### 5. Product Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/products` (changed from `/api/product`)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/products` | Get all products | ? Changed route to plural |
| GET | `/api/products/{productId}` | Get product by ID | ? Changed param from `id` to `productId` |
| POST | `/api/products` | Create product | ? Changed route to plural |
| PUT | `/api/products/{productId}` | Update product | ? Changed param |
| DELETE | `/api/products/{productId}` | Delete product | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/ProductController.cs` - Changed route to `/api/products`, updated parameter names

---

### 6. Order Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/orders` (changed from `/api/order`)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/orders` | Get all orders | ? Changed route to plural |
| GET | `/api/orders/{orderId}` | Get order by ID | ? Changed param from `id` to `orderId` |
| GET | `/api/orders/by-customer/{customerId}` | Get orders by customer | ? Added new endpoint |
| POST | `/api/orders` | Create order | ? Changed route to plural |
| PUT | `/api/orders/{orderId}` | Update order | ? Changed param |
| DELETE | `/api/orders/{orderId}` | Delete order | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/OrderController.cs` - Changed route to `/api/orders`, added `by-customer` endpoint
- `Application/Common/Interfaces/IOrderService.cs` - Changed int to Guid for IDs
- `Application/Services/OrderService.cs` - Changed int to Guid for IDs

---

### 7. Customer Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/customers` (changed from `/api/customer`)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/customers` | Get all customers | ? Changed route to plural |
| GET | `/api/customers/{customerId}` | Get customer by ID | ? Changed param from `id` to `customerId` |
| POST | `/api/customers` | Create customer | ? Changed route to plural |
| PUT | `/api/customers/{customerId}` | Update customer | ? Changed param |
| DELETE | `/api/customers/{customerId}` | Delete customer | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/CustomerController.cs` - Changed route to `/api/customers`, updated parameter names
- `Application/Common/Interfaces/ICustomerService.cs` - Changed int to Guid for IDs
- `Application/Services/CustomerService.cs` - Changed int to Guid for IDs

---

### 8. Campaign Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/campaigns`

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/campaigns/{campaignId}` | Get campaign by ID | ? Changed param from `id` to `campaignId` |
| POST | `/api/campaigns` | Create campaign | ? Already correct |
| PUT | `/api/campaigns/{campaignId}` | Update campaign | ? Changed param |
| DELETE | `/api/campaigns/{campaignId}` | Delete campaign | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/CampaignController.cs` - Updated parameter names

---

### 9. Campaign Post Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/campaign-posts` (changed from `/api/campaigns/{campaignId}/posts`)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/campaign-posts` | Get all campaign posts | ? Changed from nested to flat route |
| GET | `/api/campaign-posts/{postId}` | Get campaign post by ID | ? Changed from nested to flat route |
| POST | `/api/campaign-posts` | Create campaign post | ? Changed from nested to flat route |
| PUT | `/api/campaign-posts/{postId}` | Update campaign post | ? Changed from nested to flat route |
| DELETE | `/api/campaign-posts/{postId}` | Delete campaign post | ? Changed from nested to flat route |

**Files Modified:**
- `Presentation/Controllers/CampaignPostController.cs` - Changed from nested to flat route, removed `campaignId` from URLs
- `Application/Common/Interfaces/ICampaignPostService.cs` - Removed `campaignId` parameters, added `GetAllPostsAsync`
- `Application/Services/CampaignPostService.cs` - Updated methods to support flat routing

---

### 10. Automation Task Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/automation-tasks` (changed from `/api/[controller]`)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/automation-tasks` | Get all automation tasks | ? Changed route to kebab-case |
| GET | `/api/automation-tasks/{taskId}` | Get task by ID | ? Changed param from `id` to `taskId` |
| POST | `/api/automation-tasks` | Create automation task | ? Changed route |
| PUT | `/api/automation-tasks/{taskId}` | Update task | ? Changed param |
| DELETE | `/api/automation-tasks/{taskId}` | Delete task | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/AutomationTaskController.cs` - Changed route to `/api/automation-tasks`, updated parameter names

---

### 11. Chatbot FAQ Endpoints (STORE-SCOPED - X-Store-ID REQUIRED)

**Route:** `/api/chatbot-faq`

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/chatbot-faq` | Get all chatbot FAQs | ? Already correct |
| GET | `/api/chatbot-faq/{faqId}` | Get FAQ by ID | ? Changed param from `id` to `faqId` |
| POST | `/api/chatbot-faq` | Create FAQ | ? Already correct |
| PUT | `/api/chatbot-faq/{faqId}` | Update FAQ | ? Changed param |
| DELETE | `/api/chatbot-faq/{faqId}` | Delete FAQ | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/ChatbotFAQController.cs` - Updated parameter names

---

### 12. Social Platform Endpoints (MIXED: GLOBAL + STORE-SCOPED)

**Route:** `/api/social-platforms`

#### GLOBAL Endpoint (NO X-Store-ID)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/social-platforms` | Get available platform types | ? Added new endpoint |

#### STORE-SCOPED Endpoints (X-Store-ID REQUIRED)

| Method | Endpoint | Description | Changes |
|--------|----------|-------------|---------|
| GET | `/api/social-platforms/{connectionId}` | Get platform connection by ID | ? Changed param from `id` to `connectionId` |
| POST | `/api/social-platforms` | Create platform connection | ? Already correct |
| POST | `/api/social-platforms/facebook/connect` | Connect Facebook | ? Already correct |
| POST | `/api/social-platforms/instagram/connect` | Connect Instagram | ? Already correct |
| PUT | `/api/social-platforms/{connectionId}/disconnect` | Disconnect platform | ? Changed param |
| DELETE | `/api/social-platforms/{connectionId}` | Delete platform connection | ? Changed param |

**Files Modified:**
- `Presentation/Controllers/SocialPlatformController.cs` - Added `GET /` endpoint, updated parameter names

---

## ??? Middleware Updates

### StoreValidationMiddleware

**Updated non-store-scoped paths:**
```csharp
private static readonly string[] NonStoreScopedPaths = new[]
{
    "/api/auth",
    "/api/users",
    "/api/stores/my",        // ? Updated from /api/store/my-stores
    "/api/stores",           // ? Updated from /api/store
    "/api/teams/my",         // ? Added
    "/api/social-platforms", // ? Added (with method check for GET)
    "/swagger",
    "/favicon.ico",
    "/_framework",
    "/_vs"
};
```

**Added special handling:**
- `GET /api/social-platforms` is GLOBAL
- `POST/PUT/DELETE /api/social-platforms/*` are STORE-SCOPED

**Files Modified:**
- `Presentation/Middleware/StoreValidationMiddleware.cs`

---

### SwaggerStoreIdHeaderOperationFilter

**Updated non-store-scoped paths:**
```csharp
private static readonly string[] NonStoreScopedPaths = new[]
{
    "/api/auth",
    "/api/users",
    "/api/stores/my",   // ? Updated
    "/api/stores",      // ? Updated
    "/api/teams/my"     // ? Added
};
```

**Added special handling:**
- `GET /api/social-platforms` doesn't show X-Store-ID header in Swagger

**Files Modified:**
- `Presentation/Middleware/SwaggerStoreIdHeaderOperationFilter.cs`

---

## ?? Complete Endpoint Map

### GLOBAL Endpoints (NO X-Store-ID Required)

#### Authentication
- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/logout`

#### Users
- `GET /api/users/me`
- `GET /api/users/{id}`
- `GET /api/users/by-email/{email}`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`

#### Stores
- `GET /api/stores/my`
- `GET /api/stores/{storeId}`
- `POST /api/stores`
- `PUT /api/stores/{storeId}`
- `DELETE /api/stores/{storeId}`

#### Teams (Global)
- `GET /api/teams/my`
- `GET /api/teams/{teamId}`

#### Social Platforms (Discovery)
- `GET /api/social-platforms`

---

### STORE-SCOPED Endpoints (X-Store-ID REQUIRED)

#### Teams
- `GET /api/teams`
- `POST /api/teams`
- `PUT /api/teams/{teamId}`
- `DELETE /api/teams/{teamId}`
- `GET /api/teams/{teamId}/members`
- `POST /api/teams/{teamId}/members`
- `DELETE /api/teams/{teamId}/members/{userId}`

#### Products
- `GET /api/products`
- `GET /api/products/{productId}`
- `POST /api/products`
- `PUT /api/products/{productId}`
- `DELETE /api/products/{productId}`

#### Orders
- `GET /api/orders`
- `GET /api/orders/{orderId}`
- `GET /api/orders/by-customer/{customerId}`
- `POST /api/orders`
- `PUT /api/orders/{orderId}`
- `DELETE /api/orders/{orderId}`

#### Customers
- `GET /api/customers`
- `GET /api/customers/{customerId}`
- `POST /api/customers`
- `PUT /api/customers/{customerId}`
- `DELETE /api/customers/{customerId}`

#### Campaigns
- `GET /api/campaigns/{campaignId}`
- `POST /api/campaigns`
- `PUT /api/campaigns/{campaignId}`
- `DELETE /api/campaigns/{campaignId}`

#### Campaign Posts
- `GET /api/campaign-posts`
- `GET /api/campaign-posts/{postId}`
- `POST /api/campaign-posts`
- `PUT /api/campaign-posts/{postId}`
- `DELETE /api/campaign-posts/{postId}`

#### Automation Tasks
- `GET /api/automation-tasks`
- `GET /api/automation-tasks/{taskId}`
- `POST /api/automation-tasks`
- `PUT /api/automation-tasks/{taskId}`
- `DELETE /api/automation-tasks/{taskId}`

#### Chatbot FAQ
- `GET /api/chatbot-faq`
- `GET /api/chatbot-faq/{faqId}`
- `POST /api/chatbot-faq`
- `PUT /api/chatbot-faq/{faqId}`
- `DELETE /api/chatbot-faq/{faqId}`

#### Social Platforms
- `GET /api/social-platforms/{connectionId}`
- `POST /api/social-platforms`
- `POST /api/social-platforms/facebook/connect`
- `POST /api/social-platforms/instagram/connect`
- `PUT /api/social-platforms/{connectionId}/disconnect`
- `DELETE /api/social-platforms/{connectionId}`

---

## ?? Bug Fixes Applied

### ID Type Consistency

**Problem:** Some service interfaces used `int` for IDs while entities use `Guid`

**Fixed:**
- `ICustomerService` - Changed `int id` to `Guid id`
- `IOrderService` - Changed `int id` to `Guid id`
- `CustomerService` - Updated implementation
- `OrderService` - Updated implementation

**Affected Methods:**
- `GetByIdAsync`
- `GetByCustomerIdAsync` (Order service)
- `UpdateAsync`
- `DeleteAsync`

---

## ?? Architectural Principles Maintained

1. **? Flat REST URLs** - No nested routes (e.g., `/api/campaign-posts` instead of `/api/campaigns/{id}/posts`)

2. **? Plural Resource Naming** - All routes use plural forms (`/api/products`, `/api/orders`, etc.)

3. **? Consistent Parameter Naming** - Specific ID names (`storeId`, `teamId`, `productId`, etc.) instead of generic `id`

4. **? Clear Scope Separation** - GLOBAL vs STORE-SCOPED clearly documented in XML comments

5. **? GUID Usage** - All entity IDs use `Guid` (from `BaseEntity`)

6. **? Store Context via Header** - StoreId NEVER in JWT, always in `X-Store-ID` header

7. **? Middleware-Based Filtering** - Store context set by middleware, query filters handle automatic filtering

8. **? Clean Architecture** - Controllers thin, business logic in services, authorization in middleware

---

## ??? Build Status

? **Build Successful** - No compilation errors  
? **All controllers updated** - Routes, parameters, and comments  
? **All service interfaces updated** - Method signatures corrected  
? **All service implementations updated** - Logic matches interfaces  
? **Middleware updated** - Path lists and logic updated  
? **ID types fixed** - Consistent Guid usage throughout  

---

## ?? Testing Checklist

### GLOBAL Endpoints (Test WITHOUT X-Store-ID)

- [ ] `POST /api/auth/login`
- [ ] `POST /api/auth/register`
- [ ] `POST /api/auth/logout`
- [ ] `GET /api/users/me`
- [ ] `GET /api/stores/my`
- [ ] `POST /api/stores`
- [ ] `GET /api/teams/my`
- [ ] `GET /api/social-platforms`

### STORE-SCOPED Endpoints (Test WITH X-Store-ID)

- [ ] `GET /api/products`
- [ ] `POST /api/products`
- [ ] `GET /api/orders`
- [ ] `GET /api/orders/by-customer/{customerId}`
- [ ] `GET /api/customers`
- [ ] `GET /api/campaigns/{campaignId}`
- [ ] `GET /api/campaign-posts`
- [ ] `GET /api/teams` (store teams)
- [ ] `POST /api/teams`
- [ ] `POST /api/social-platforms/facebook/connect`

### Error Scenarios

- [ ] Try store-scoped endpoint WITHOUT X-Store-ID ? Should return 401 Unauthorized
- [ ] Try store-scoped endpoint with INVALID X-Store-ID ? Should return 404 Not Found
- [ ] Try store-scoped endpoint with store user doesn't own ? Should return 403 Forbidden

---

## ?? Frontend Integration Notes

### Angular HTTP Interceptor

```typescript
export class StoreContextInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const storeId = localStorage.getItem('selectedStoreId');
    
    // Non-store-scoped paths (NO X-Store-ID)
    const nonStoreScopedPaths = [
      '/api/auth',
      '/api/users',
      '/api/stores/my',
      '/api/stores',
      '/api/teams/my'
    ];
    
    // Check if request is to non-store-scoped endpoint
    const isNonStoreScoped = nonStoreScopedPaths.some(path => req.url.includes(path));
    
    // Special case: GET /api/social-platforms is global
    const isSocialPlatformsDiscovery = req.url.includes('/api/social-platforms') 
      && req.method === 'GET' 
      && !req.url.match(/\/api\/social-platforms\/[a-f0-9-]+/);
    
    if (isNonStoreScoped || isSocialPlatformsDiscovery) {
      return next.handle(req);
    }
    
    // Add X-Store-ID for store-scoped endpoints
    if (storeId) {
      req = req.clone({
        setHeaders: { 'X-Store-ID': storeId }
      });
    }
    
    return next.handle(req);
  }
}
```

### Updated Service Calls

```typescript
// BEFORE: Nested routes
getCampaignPosts(campaignId: string) {
  return this.http.get(`/api/campaigns/${campaignId}/posts`);
}

// AFTER: Flat routes
getCampaignPosts() {
  // X-Store-ID header added by interceptor
  return this.http.get('/api/campaign-posts');
}
```

---

## ?? Summary

All endpoints have been successfully adapted to match your business requirements:

- ? **Flat REST URLs** throughout
- ? **Plural resource naming** consistent
- ? **Clear GLOBAL vs STORE-SCOPED** distinction
- ? **Consistent parameter naming** (storeId, teamId, etc.)
- ? **GUID usage** for all IDs
- ? **Middleware updated** for new routes
- ? **Service interfaces** corrected
- ? **Build successful** with no errors

Your API now follows RESTful best practices while maintaining your specific business logic for store-scoped operations!
