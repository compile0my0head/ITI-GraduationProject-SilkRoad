import { Routes } from '@angular/router';
import { authGuard, guestGuard, storeGuard } from './core/auth/guards/auth.guard';

// Layouts
import { LayoutComponent } from './shared/components/layout/layout.component';
import { PublicLayoutComponent } from './shared/layouts/public-layout/public-layout.component';

// Landing (public)
import { LandingComponent } from './features/landing/landing.component';

// Auth
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';

// Home (store selection)
import { StoreHomeComponent } from './features/stores/store-home/store-home.component';

// Dashboard
import { DashboardComponent } from './features/dashboard/dashboard.component';

// Products
import { ProductsListComponent } from './features/products/list/products-list.component';
import { AddProductComponent } from './features/products/add/add-product.component';
import { EditProductComponent } from './features/products/edit/edit-product.component';

// Orders
import { OrdersListComponent } from './features/orders/list/orders-list.component';
import { OrderDetailsComponent } from './features/orders/details/order-details.component';

// Customers
import { CustomersListComponent } from './features/customers/list/customers-list.component';
import { AddCustomerComponent } from './features/customers/add/add-customer.component';
import { EditCustomerComponent } from './features/customers/edit/edit-customer.component';
import { CustomerDetailsComponent } from './features/customers/details/customer-details.component';

// Campaigns
import { CampaignsListComponent } from './features/campaigns/list/campaigns-list.component';
import { CreateCampaignComponent } from './features/campaigns/create/create-campaign.component';
import { EditCampaignComponent } from './features/campaigns/edit/edit-campaign.component';

// Campaign Posts
import { CampaignPostsListComponent } from './features/campaign-posts/list/campaign-posts-list.component';
import { CreateCampaignPostComponent } from './features/campaign-posts/create/create-campaign-post.component';
import { EditCampaignPostComponent } from './features/campaign-posts/edit/edit-campaign-post.component';

// Platforms
import { PlatformsListComponent } from './features/platforms/list/platforms-list.component';
import { ConnectPlatformComponent } from './features/platforms/connect/connect-platform.component';

// Teams (placeholder)
import { TeamsListComponent } from './features/teams/list/teams-list.component';

export const routes: Routes = [
  // Public routes with PublicLayoutComponent (navbar + router-outlet)
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      // Landing page
      { path: '', component: LandingComponent, pathMatch: 'full' },
      // Auth routes (guest only)
      {
        path: 'auth',
        canActivate: [guestGuard],
        children: [
          { path: 'login', component: LoginComponent },
          { path: 'register', component: RegisterComponent },
          { path: '', redirectTo: 'login', pathMatch: 'full' }
        ]
      }
    ]
  },

  // Home - Store selection (authenticated, no store required)
  {
    path: 'home',
    component: StoreHomeComponent,
    canActivate: [authGuard]
  },

  // Protected routes with layout (require auth + store)
  {
    path: 'app',
    component: LayoutComponent,
    canActivate: [authGuard, storeGuard],
    children: [
      // Dashboard
      { path: 'dashboard', component: DashboardComponent },

      // Products
      { path: 'products', component: ProductsListComponent },
      { path: 'products/add', component: AddProductComponent },
      { path: 'products/edit/:id', component: EditProductComponent },

      // Orders
      { path: 'orders', component: OrdersListComponent },
      { path: 'orders/:id', component: OrderDetailsComponent },

      // Customers
      { path: 'customers', component: CustomersListComponent },
      { path: 'customers/add', component: AddCustomerComponent },
      { path: 'customers/:id', component: CustomerDetailsComponent },
      { path: 'customers/edit/:id', component: EditCustomerComponent },

      // Campaigns
      { path: 'campaigns', component: CampaignsListComponent },
      { path: 'campaigns/create', component: CreateCampaignComponent },
      { path: 'campaigns/edit/:id', component: EditCampaignComponent },
      { path: 'campaigns/:campaignId/posts', component: CampaignPostsListComponent },
      { path: 'campaigns/:campaignId/posts/create', component: CreateCampaignPostComponent },
      { path: 'campaigns/:campaignId/posts/edit/:postId', component: EditCampaignPostComponent },

      // Platforms
      { path: 'platforms', component: PlatformsListComponent },
      { path: 'platforms/connect/:platform', component: ConnectPlatformComponent },

      // Teams (placeholder - coming soon)
      { path: 'teams', component: TeamsListComponent },

      // Default redirect to dashboard
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },

  // Catch-all redirects to landing
  { path: '**', redirectTo: '' }
];
