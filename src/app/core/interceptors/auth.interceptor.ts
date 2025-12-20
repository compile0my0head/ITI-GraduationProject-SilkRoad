import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { StoreContextService } from '../services/store-context.service';

// Global endpoints that don't require X-Store-ID header
const GLOBAL_ENDPOINTS = [
  '/api/auth/',
  '/api/users/',
  '/api/stores/my',
  '/api/stores', // POST to create store
  '/api/teams/my',
  '/api/social-platforms/available-platforms'
];

// Regex to match GET /api/stores/{guid}
const STORE_BY_ID_REGEX = /\/stores\/[a-f0-9-]+$/i;

function isGlobalEndpoint(url: string, method: string): boolean {
  // Check if URL matches any global endpoint pattern
  for (const endpoint of GLOBAL_ENDPOINTS) {
    if (url.includes(endpoint)) {
      // Special case: /api/stores is global only for POST (create) and GET /my
      if (endpoint === '/api/stores') {
        // GET /api/stores/{id} and PUT /api/stores/{id} are also global for store details
        return method === 'POST' || url.includes('/stores/my') || STORE_BY_ID_REGEX.test(url);
      }
      return true;
    }
  }
  return false;
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storeContext = inject(StoreContextService);
  
  // Get token from localStorage
  const token = localStorage.getItem('silkroad_token');
  const storeId = storeContext.getStoreIdForHeader();

  let headers = req.headers;

  // Add Authorization header if token exists
  if (token) {
    headers = headers.set('Authorization', `Bearer ${token}`);
  }

  // Add X-Store-ID header for store-scoped endpoints only
  if (storeId && !isGlobalEndpoint(req.url, req.method)) {
    headers = headers.set('X-Store-ID', storeId);
  }

  const clonedReq = req.clone({ headers });
  return next(clonedReq);
};
