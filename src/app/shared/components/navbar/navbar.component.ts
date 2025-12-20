import { Component, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { AuthService } from '../../../core/auth/services/auth.service';
import { StoreService } from '../../../features/stores/services/store.service';
import { StoreContextService } from '../../../core/services/store-context.service';
import { NavbarStateService } from '../../services/navbar-state.service';
import { filter } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  disabled?: boolean;
  tooltip?: string;
}

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  private authService = inject(AuthService);
  private storeService = inject(StoreService);
  private storeContext = inject(StoreContextService);
  private navbarState = inject(NavbarStateService);
  router = inject(Router);

  // Scroll-based visibility state (from service for layout access)
  readonly isHidden = this.navbarState.isHidden;
  private lastScrollY = 0;
  private scrollThreshold = 50;

  // Sidebar state (from service for layout access)
  readonly sidebarCollapsed = this.navbarState.isCollapsed;
  readonly showStoreDropdown = signal(false);
  readonly showUserDropdown = signal(false);

  // State from services
  readonly user = this.authService.user;
  readonly currentStore = this.storeService.currentStore;
  readonly stores = this.storeService.stores;

  // Current route tracking using toSignal
  private routerEvents$ = this.router.events.pipe(
    filter((event): event is NavigationEnd => event instanceof NavigationEnd)
  );
  private navigationEnd = toSignal(this.routerEvents$);
  
  readonly currentRoute = computed(() => {
    const navEnd = this.navigationEnd();
    return navEnd?.urlAfterRedirects || this.router.url;
  });

  // Navigation items
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/app/dashboard' },
    { label: 'Products', icon: 'products', route: '/app/products' },
    { label: 'Orders', icon: 'orders', route: '/app/orders' },
    { label: 'Customers', icon: 'customers', route: '/app/customers' },
    { label: 'Campaigns', icon: 'campaigns', route: '/app/campaigns' },
    { label: 'Analytics', icon: 'analytics', route: '/app/analytics', disabled: true, tooltip: 'Coming soon' },
    { label: 'Teams', icon: 'teams', route: '/app/teams', disabled: true, tooltip: 'Coming soon' },
  ];

  @HostListener('window:scroll')
  onWindowScroll(): void {
    const scrollY = window.pageYOffset || document.documentElement.scrollTop || 0;
    
    if (scrollY > this.scrollThreshold) {
      if (scrollY > this.lastScrollY) {
        // Scrolling DOWN - hide navbar
        this.navbarState.setHidden(true);
      } else if (scrollY < this.lastScrollY) {
        // Scrolling UP - show navbar
        this.navbarState.setHidden(false);
      }
    } else {
      // At top - always show navbar
      this.navbarState.setHidden(false);
    }
    
    this.lastScrollY = scrollY <= 0 ? 0 : scrollY;
  }

  toggleSidebar(): void {
    this.navbarState.toggleCollapsed();
  }

  toggleStoreDropdown(): void {
    this.showStoreDropdown.update(v => !v);
    this.showUserDropdown.set(false);
  }

  toggleUserDropdown(): void {
    this.showUserDropdown.update(v => !v);
    this.showStoreDropdown.set(false);
  }

  selectStore(storeId: string): void {
    this.storeContext.setCurrentStoreId(storeId);
    this.showStoreDropdown.set(false);
    const currentUrl = this.router.url;
    this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
      this.router.navigate([currentUrl]);
    });
  }

  goToHome(): void {
    this.storeContext.clearStore();
    this.router.navigate(['/home']);
  }

  goToLanding(): void {
    localStorage.removeItem('currentStoreId');
    this.storeContext.clearStore();
    this.router.navigate(['/']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  closeDropdowns(): void {
    this.showStoreDropdown.set(false);
    this.showUserDropdown.set(false);
  }
}
