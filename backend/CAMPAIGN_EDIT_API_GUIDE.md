# ?? Campaign Edit API - Complete Guide

## Overview

The campaign edit functionality provides **two dedicated endpoints** for editing campaigns:

1. **GET for pre-filling edit form** - Fetches current campaign data
2. **PUT for saving changes** - Updates campaign and returns updated data

---

## ?? Endpoints

### 1. Get Campaign for Editing (Pre-fill Form)

**Purpose**: Fetch current campaign data to populate the edit form

**Endpoint**: `GET /api/campaigns/{campaignId}/edit`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```http
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

**Error Responses**:
- **404 Not Found**: Campaign not found
- **401 Unauthorized**: Invalid token
- **400 Bad Request**: Missing X-Store-ID header

---

### 2. Update Campaign (Save Changes)

**Purpose**: Update campaign and receive updated data in response

**Endpoint**: `PUT /api/campaigns/{campaignId}`  
**Type**: STORE-SCOPED  
**Authentication**: Required (Bearer Token)

**Headers**:
```http
Authorization: Bearer {token}
X-Store-ID: {store-guid}
Content-Type: application/json
```

**Path Parameters**:
- `campaignId` (GUID, required): Campaign identifier

**Request Body** (UpdateCampaignRequest - all fields optional):
```json
{
  "campaignName": "Summer Sale 2024 Extended",
  "campaignDescription": "60% off summer collection - Extended!",
  "campaignStage": "Scheduled",
  "campaignBannerUrl": "https://example.com/new-banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-45, Fashion enthusiasts, New customers",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-09-30T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "new-product-guid"
}
```

**Success Response** (200 OK) - **Returns Updated Campaign**:
```json
{
  "id": "campaign-guid",
  "campaignName": "Summer Sale 2024 Extended",
  "campaignDescription": "60% off summer collection - Extended!",
  "campaignStage": "Scheduled",
  "campaignBannerUrl": "https://example.com/new-banner.jpg",
  "goal": "Sales",
  "targetAudience": "18-45, Fashion enthusiasts, New customers",
  "scheduledStartAt": "2024-06-01T00:00:00Z",
  "scheduledEndAt": "2024-09-30T23:59:59Z",
  "isSchedulingEnabled": true,
  "assignedProductId": "new-product-guid",
  "assignedProductName": "Updated Product Name",
  "createdByUserId": "user-guid",
  "createdByUserName": "John Doe",
  "storeId": "store-guid",
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-12-18T15:30:00Z"  // ? Updated timestamp
}
```

**Error Responses**:
- **404 Not Found**: Campaign not found
- **400 Bad Request**: Validation errors (e.g., invalid product ID)
- **401 Unauthorized**: Invalid token

---

## ?? Complete Edit Workflow

### Frontend Implementation

```javascript
// ===== STEP 1: Load Campaign for Editing =====
const loadCampaignForEdit = async (campaignId) => {
  const response = await fetch(`/api/campaigns/${campaignId}/edit`, {
    headers: {
      'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
      'X-Store-ID': localStorage.getItem('currentStoreId')
    }
  });
  
  if (!response.ok) {
    throw new Error('Campaign not found');
  }
  
  const campaign = await response.json();
  
  // Pre-fill form fields
  setFormData({
    campaignName: campaign.campaignName,
    campaignDescription: campaign.campaignDescription,
    campaignStage: campaign.campaignStage,
    campaignBannerUrl: campaign.campaignBannerUrl,
    goal: campaign.goal,
    targetAudience: campaign.targetAudience,
    scheduledStartAt: campaign.scheduledStartAt,
    scheduledEndAt: campaign.scheduledEndAt,
    isSchedulingEnabled: campaign.isSchedulingEnabled,
    assignedProductId: campaign.assignedProductId
  });
  
  return campaign;
};

// ===== STEP 2: Update Campaign =====
const updateCampaign = async (campaignId, updatedData) => {
  const response = await fetch(`/api/campaigns/${campaignId}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
      'X-Store-ID': localStorage.getItem('currentStoreId'),
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(updatedData)
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message || 'Update failed');
  }
  
  const updatedCampaign = await response.json();
  
  // ? Response contains updated campaign data
  console.log('Campaign updated:', updatedCampaign);
  console.log('New updatedAt:', updatedCampaign.updatedAt);
  
  return updatedCampaign;
};

// ===== COMPLETE EDIT FLOW =====
const handleEditCampaign = async (campaignId) => {
  try {
    // 1. Load current data
    const currentCampaign = await loadCampaignForEdit(campaignId);
    
    // 2. User edits form
    // (UI renders form with pre-filled data)
    
    // 3. User submits changes
    const updatedData = {
      campaignName: formData.campaignName,
      campaignDescription: formData.campaignDescription,
      // Only include fields that changed (or send all)
    };
    
    // 4. Save changes and get updated campaign
    const savedCampaign = await updateCampaign(campaignId, updatedData);
    
    // 5. Update UI with returned data
    displaySuccess(`Campaign updated: ${savedCampaign.campaignName}`);
    redirectToCampaignList(); // or display updated campaign
    
  } catch (error) {
    console.error('Edit failed:', error);
    displayError(error.message);
  }
};
```

---

## ?? React Example (Hooks)

```jsx
import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';

const EditCampaignPage = () => {
  const { campaignId } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [formData, setFormData] = useState({});
  
  // ===== Load Campaign on Mount =====
  useEffect(() => {
    loadCampaign();
  }, [campaignId]);
  
  const loadCampaign = async () => {
    try {
      const response = await fetch(`/api/campaigns/${campaignId}/edit`, {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
          'X-Store-ID': localStorage.getItem('currentStoreId')
        }
      });
      
      if (!response.ok) throw new Error('Campaign not found');
      
      const campaign = await response.json();
      setFormData(campaign); // ? Pre-fill form
      setLoading(false);
    } catch (error) {
      alert(error.message);
      navigate('/campaigns');
    }
  };
  
  // ===== Save Changes =====
  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const response = await fetch(`/api/campaigns/${campaignId}`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('authToken')}`,
          'X-Store-ID': localStorage.getItem('currentStoreId'),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formData)
      });
      
      if (!response.ok) throw new Error('Update failed');
      
      const updatedCampaign = await response.json(); // ? Get updated data
      
      alert(`Campaign updated: ${updatedCampaign.campaignName}`);
      navigate('/campaigns');
    } catch (error) {
      alert(error.message);
      setLoading(false);
    }
  };
  
  if (loading) return <div>Loading...</div>;
  
  return (
    <form onSubmit={handleSubmit}>
      <h1>Edit Campaign</h1>
      
      <input
        type="text"
        value={formData.campaignName || ''}
        onChange={(e) => setFormData({ ...formData, campaignName: e.target.value })}
        placeholder="Campaign Name"
        required
      />
      
      <textarea
        value={formData.campaignDescription || ''}
        onChange={(e) => setFormData({ ...formData, campaignDescription: e.target.value })}
        placeholder="Description"
      />
      
      <select
        value={formData.campaignStage || 'Draft'}
        onChange={(e) => setFormData({ ...formData, campaignStage: e.target.value })}
      >
        <option value="Draft">Draft</option>
        <option value="InReview">In Review</option>
        <option value="Scheduled">Scheduled</option>
        <option value="Ready">Ready</option>
        <option value="Published">Published</option>
      </select>
      
      {/* Add more fields... */}
      
      <button type="submit" disabled={loading}>
        {loading ? 'Saving...' : 'Save Changes'}
      </button>
    </form>
  );
};

export default EditCampaignPage;
```

---

## ?? Editable Fields

### CampaignDto Fields (All Returned)

| Field | Type | Editable | Description |
|-------|------|----------|-------------|
| `id` | GUID | ? No | Campaign identifier (read-only) |
| `campaignName` | string | ? Yes | Campaign name (max 200 chars) |
| `campaignDescription` | string | ? Yes | Campaign description |
| `campaignStage` | string | ? Yes | Stage: Draft, InReview, Scheduled, Ready, Published |
| `campaignBannerUrl` | string | ? Yes | Banner image URL (max 500 chars) |
| `goal` | string | ? Yes | Campaign goal (max 100 chars) |
| `targetAudience` | string | ? Yes | Target audience description |
| `scheduledStartAt` | DateTime | ? Yes | Campaign start date/time |
| `scheduledEndAt` | DateTime | ? Yes | Campaign end date/time |
| `isSchedulingEnabled` | boolean | ? Yes | Enable/disable scheduling |
| `assignedProductId` | GUID | ? Yes | Featured product ID |
| `assignedProductName` | string | ? No | Product name (read-only, auto-loaded) |
| `createdByUserId` | GUID | ? No | Creator user ID (read-only) |
| `createdByUserName` | string | ? No | Creator name (read-only) |
| `storeId` | GUID | ? No | Store ID (read-only) |
| `createdAt` | DateTime | ? No | Creation timestamp (read-only) |
| `updatedAt` | DateTime | ? No | Last update timestamp (read-only, auto-updated) |

---

## ? Response Guarantees

### What the API Returns

? **Always returns the complete updated campaign**  
? **Includes all navigation properties** (assignedProductName, createdByUserName)  
? **Updated timestamp** reflects the save time  
? **All fields validated** before save  
? **Atomic operation** (all-or-nothing update)

---

## ?? Common Errors & Solutions

### Error 1: 404 Not Found
**Cause**: Campaign doesn't exist or doesn't belong to current store

**Solution**:
```javascript
if (response.status === 404) {
  alert('Campaign not found or you don\'t have access');
  navigate('/campaigns');
}
```

### Error 2: 400 Bad Request (Invalid Product)
**Cause**: `assignedProductId` doesn't belong to current store

**Response**:
```json
{
  "message": "Product {productId} does not belong to store {storeId}."
}
```

**Solution**: Validate product selection against store's products

### Error 3: 400 Bad Request (Validation Error)
**Cause**: Invalid data (e.g., campaignName too long)

**Response**:
```json
{
  "campaignName": ["Campaign name must be between 3 and 200 characters"]
}
```

**Solution**: Add frontend validation matching backend rules

---

## ?? Best Practices

### 1. Always Use the Edit Endpoint for Pre-filling
```javascript
// ? Good: Use dedicated edit endpoint
const campaign = await fetch(`/api/campaigns/${id}/edit`);

// ? Avoid: Using list endpoint and filtering client-side
const campaigns = await fetch('/api/campaigns');
const campaign = campaigns.find(c => c.id === id);
```

### 2. Handle Partial Updates
```javascript
// Only send changed fields
const changedFields = {
  campaignName: newName !== originalName ? newName : undefined,
  campaignDescription: newDesc !== originalDesc ? newDesc : undefined
};

// Remove undefined fields
const updatePayload = Object.fromEntries(
  Object.entries(changedFields).filter(([_, v]) => v !== undefined)
);

await updateCampaign(campaignId, updatePayload);
```

### 3. Optimistic UI Updates
```javascript
// 1. Update UI immediately
setCampaign({ ...campaign, campaignName: newName });

try {
  // 2. Save to server
  const updated = await updateCampaign(campaignId, { campaignName: newName });
  
  // 3. Confirm with server response
  setCampaign(updated);
} catch (error) {
  // 4. Revert on error
  setCampaign(originalCampaign);
  alert('Update failed');
}
```

### 4. Show Loading States
```jsx
<button disabled={loading}>
  {loading ? 'Saving...' : 'Save Changes'}
</button>
```

---

## ?? Data Flow Diagram

```
???????????????????????????????????????????????????????????
?                    Frontend                              ?
?                                                          ?
?  1. User clicks "Edit Campaign"                         ?
?     ?                                                    ?
?  2. GET /api/campaigns/{id}/edit                        ?
?     ?                                                    ?
?  3. ? Receive current campaign data                    ?
?     ?                                                    ?
?  4. Pre-fill form with data                             ?
?     ?                                                    ?
?  5. User edits fields                                   ?
?     ?                                                    ?
?  6. PUT /api/campaigns/{id}                             ?
?     {                                                    ?
?       campaignName: "New Name",                         ?
?       campaignDescription: "Updated description"        ?
?     }                                                    ?
?     ?                                                    ?
?  7. ? Receive updated campaign                         ?
?     {                                                    ?
?       id: "...",                                        ?
?       campaignName: "New Name",                         ?
?       updatedAt: "2024-12-18T15:30:00Z"                ?
?     }                                                    ?
?     ?                                                    ?
?  8. Update UI with returned data                        ?
?     ?                                                    ?
?  9. Show success message                                ?
?                                                          ?
???????????????????????????????????????????????????????????
```

---

## ?? Security Notes

1. **Authentication Required**: Both endpoints require valid JWT token
2. **Store Scoping**: Campaign must belong to X-Store-ID header
3. **Authorization**: User must have access to the store
4. **Validation**: All input validated server-side
5. **Soft Delete**: Deleted campaigns filtered automatically

---

## ?? Summary

? **GET `/api/campaigns/{id}/edit`** - Fetch current campaign data for form  
? **PUT `/api/campaigns/{id}`** - Update campaign and receive updated data  
? **Response always contains updated campaign** with all fields  
? **Navigation properties auto-loaded** (product name, creator name)  
? **Timestamps updated automatically**  

**Your campaign edit API is fully functional and returns complete data!** ??

