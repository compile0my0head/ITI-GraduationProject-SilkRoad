# Platform Dropdown API Implementation

## ? What Was Implemented

### 1. **API Endpoint: GET /api/social-platforms/available-platforms**

**Purpose:** Returns all available social platforms from the `PlatformName` enum for frontend dropdown population.

**Authentication:** Requires JWT Bearer token

**Request:**
```http
GET /api/social-platforms/available-platforms
Authorization: Bearer {your-jwt-token}
```

**Response Example:**
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

**Response Fields:**
- `value` (int): Enum numeric value (0-3) - can be used for database inserts
- `name` (string): Enum string name - matches PlatformName enum values
- `displayName` (string): Human-readable name for UI display
- `isOAuthEnabled` (bool): Indicates if OAuth integration is ready (currently only Facebook = true)

---

## ?? Required Fields Explanation for `/api/social-platforms/facebook/connect`

When you implement OAuth (future), this endpoint receives:

### 1. **`code`** (string) - **REQUIRED**
- **What it is:** OAuth authorization code returned by Facebook after user approval
- **How you get it:** Facebook redirects user back to your callback URL with `?code=...` in the query string
- **What it's used for:** Your backend exchanges this code for a long-lived access token
- **OAuth Flow:**
  1. User clicks "Connect" ? Redirect to Facebook
  2. User approves permissions on Facebook
  3. Facebook redirects back: `https://yourapp.com/callback?code=ABC123XYZ`
  4. Frontend sends this `code` to your API
  5. API exchanges `code` for `access_token` with Facebook Graph API

### 2. **`redirectUri`** (string) - **OPTIONAL**
- **What it is:** The callback URL where Facebook redirects the user after authorization
- **Must match:** The redirect URI configured in your Facebook App Dashboard
- **Default value:** Uses `Facebook:RedirectUri` from `appsettings.json` if not provided
- **Example:** `"https://localhost:5001/facebook-callback.html"`

---

## ?? MVP Business Model Demonstration

### Current Implementation Status

? **Ready for Demo:**
- Platform dropdown populated dynamically from backend enum
- All 4 platforms displayed (Facebook, Instagram, TikTok, YouTube)
- UI shows which platforms have OAuth enabled
- "Connect to Platform" button ready
- Facebook OAuth flow ready (when configured)

?? **Coming Soon (Not MVP):**
- Instagram OAuth integration
- TikTok OAuth integration
- YouTube OAuth integration

### How to Demo Your MVP

1. **Show the dropdown:**
   - Open `https://localhost:5001/platform-dropdown-demo.html`
   - Dropdown auto-populates with all platforms from backend

2. **Demonstrate platform selection:**
   - Select different platforms from dropdown
   - Show OAuth status badge (Facebook = Ready, others = Coming Soon)

3. **Explain the business value:**
   - "Our SaaS allows stores to connect multiple social media platforms"
   - "Currently supporting Facebook with more platforms in development"
   - "One-click connection via OAuth - secure and user-friendly"

4. **Manual Testing (Before OAuth is live):**
   - For demo purposes, you can manually insert platform connections in database
   - See SQL script below

---

## ??? Manual Database Insert for Testing

Until OAuth is implemented, insert test data directly:

```sql
-- Insert a test Facebook connection for your store
INSERT INTO SocialPlatforms (
    Id,
    StoreId,
    PlatformName,
    ExternalPageID,
    PageName,
    AccessToken,
    IsConnected,
    IsDeleted,
    CreatedAt,
    UpdatedAt
)
VALUES (
    NEWID(),                           -- Generates a new GUID
    'YOUR_STORE_ID_HERE',              -- ?? Replace with your test store GUID
    0,                                 -- 0 = Facebook (from PlatformName enum)
    '123456789012345',                 -- Facebook Page ID (dummy for testing)
    'My Test Store Page',              -- Page name shown in UI
    'TEST_ACCESS_TOKEN_12345',         -- Dummy access token (won't work for real posts)
    1,                                 -- IsConnected = true
    0,                                 -- IsDeleted = false
    GETUTCDATE(),                      -- CreatedAt
    GETUTCDATE()                       -- UpdatedAt
);

-- Insert Instagram (not OAuth-enabled yet)
INSERT INTO SocialPlatforms (
    Id, StoreId, PlatformName, ExternalPageID, PageName, 
    AccessToken, IsConnected, IsDeleted, CreatedAt, UpdatedAt
)
VALUES (
    NEWID(),
    'YOUR_STORE_ID_HERE',
    1,                                 -- 1 = Instagram
    '987654321098765',
    'My Instagram Business',
    'TEST_IG_TOKEN',
    1, 0, GETUTCDATE(), GETUTCDATE()
);
```

### PlatformName Enum Values (for database):
- `0` = Facebook
- `1` = Instagram
- `2` = TikTok
- `3` = YouTube

---

## ?? Frontend Integration Example

### JavaScript Fetch Example:
```javascript
// Fetch available platforms for dropdown
async function loadPlatforms() {
    const response = await fetch('https://localhost:5001/api/social-platforms/available-platforms', {
        headers: {
            'Authorization': 'Bearer YOUR_JWT_TOKEN',
            'Content-Type': 'application/json'
        }
    });
    
    const platforms = await response.json();
    
    // Populate dropdown
    const dropdown = document.getElementById('platformDropdown');
    platforms.forEach(platform => {
        const option = document.createElement('option');
        option.value = platform.name;
        option.textContent = platform.displayName;
        
        // Add visual indicator for OAuth readiness
        if (!platform.isOAuthEnabled) {
            option.textContent += ' (Coming Soon)';
        }
        
        dropdown.appendChild(option);
    });
}
```

### React Example:
```jsx
import { useEffect, useState } from 'react';

function PlatformDropdown() {
    const [platforms, setPlatforms] = useState([]);
    const [selected, setSelected] = useState('');

    useEffect(() => {
        fetch('https://localhost:5001/api/social-platforms/available-platforms', {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}`,
            }
        })
        .then(res => res.json())
        .then(data => setPlatforms(data));
    }, []);

    const handleConnect = () => {
        const platform = platforms.find(p => p.name === selected);
        
        if (platform?.isOAuthEnabled && platform.name === 'Facebook') {
            // Redirect to Facebook OAuth
            const facebookAuthUrl = 
                `https://www.facebook.com/v18.0/dialog/oauth?` +
                `client_id=${process.env.FACEBOOK_APP_ID}` +
                `&redirect_uri=${encodeURIComponent(process.env.REDIRECT_URI)}` +
                `&scope=pages_show_list,pages_manage_posts` +
                `&response_type=code`;
            
            window.location.href = facebookAuthUrl;
        } else {
            alert(`${selected} integration coming soon!`);
        }
    };

    return (
        <div>
            <select value={selected} onChange={e => setSelected(e.target.value)}>
                <option value="">Choose Platform</option>
                {platforms.map(p => (
                    <option key={p.value} value={p.name}>
                        {p.displayName} {!p.isOAuthEnabled && '(Coming Soon)'}
                    </option>
                ))}
            </select>
            <button onClick={handleConnect} disabled={!selected}>
                Connect to Platform
            </button>
        </div>
    );
}
```

---

## ?? OAuth Flow (When You Implement It)

### Complete Flow:
1. **User selects platform** ? Dropdown populated from `/available-platforms`
2. **User clicks "Connect"** ? Redirect to Facebook OAuth URL
3. **Facebook authorization** ? User approves permissions
4. **Facebook callback** ? Redirects to your callback URL with `code`
5. **Frontend sends code** ? POST to `/api/social-platforms/facebook/connect`
6. **Backend exchanges code** ? Calls Facebook Graph API to get `access_token`
7. **Backend stores connection** ? Creates `SocialPlatform` record in database
8. **Success response** ? Returns platform details to frontend

### Swagger Testing (When OAuth is ready):
```json
POST /api/social-platforms/facebook/connect
Headers:
  Authorization: Bearer {your-jwt-token}
  X-Store-ID: {your-store-guid}
  
Body:
{
  "code": "AQBx7s9kFJ...",
  "redirectUri": "https://localhost:5001/facebook-callback.html"
}
```

---

## ?? Files Modified/Created

### Modified:
- `Presentation/Controllers/SocialPlatformController.cs`
  - Updated `GetAvailablePlatforms()` endpoint to use `PlatformName` enum
  - Added `using Domain.Enums;`
  - Improved response format with `isOAuthEnabled` flag

### Created:
- `Presentation/wwwroot/platform-dropdown-demo.html`
  - Demo page showing dropdown functionality
  - Ready for MVP demonstration
  - Includes OAuth flow (when configured)

---

## ? Testing Checklist

- [ ] API returns all 4 platforms from enum
- [ ] JWT authentication required
- [ ] Response includes `isOAuthEnabled` flag (Facebook = true)
- [ ] Swagger UI shows endpoint documentation
- [ ] Frontend can populate dropdown from API
- [ ] Manual database insert works for testing
- [ ] Demo page loads and displays platforms

---

## ?? Next Steps (Post-MVP)

1. **Configure Facebook App** (to enable real OAuth):
   - Create Facebook App in Meta Developer Console
   - Add credentials to `appsettings.json`:
     ```json
     "Facebook": {
       "AppId": "your-app-id",
       "AppSecret": "your-app-secret",
       "RedirectUri": "https://localhost:5001/facebook-callback.html"
     }
     ```

2. **Implement Instagram OAuth**:
   - Instagram uses Facebook Business API
   - Similar flow to Facebook

3. **Implement TikTok OAuth**:
   - Requires TikTok Developer account
   - Different OAuth flow

4. **Implement YouTube OAuth**:
   - Uses Google OAuth
   - Requires Google Cloud project

---

## ?? Support

If you encounter issues:
1. Check JWT token is valid
2. Verify API is running (`dotnet run` in Presentation folder)
3. Check browser console for errors
4. Test API directly in Swagger UI first
