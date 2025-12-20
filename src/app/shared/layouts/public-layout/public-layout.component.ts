import { Component, inject, signal, computed, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/auth/services/auth.service';
import { StoreContextService } from '../../../core/services/store-context.service';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './public-layout.component.html',
  styleUrls: ['./public-layout.component.scss']
})
export class PublicLayoutComponent {
  router = inject(Router);
  private authService = inject(AuthService);
  private storeContext = inject(StoreContextService);

  // Auth state
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly user = this.authService.user;

  // Scroll-based visibility
  readonly isHidden = signal(false);
  readonly isScrolled = signal(false);
  private lastScrollY = 0;
  private scrollThreshold = 50;

  // Current route tracking
  private routerEvents$ = this.router.events.pipe(
    filter((event): event is NavigationEnd => event instanceof NavigationEnd)
  );
  private navigationEnd = toSignal(this.routerEvents$);
  
  readonly currentRoute = computed(() => {
    const navEnd = this.navigationEnd();
    return navEnd?.urlAfterRedirects || this.router.url;
  });

  readonly isLandingPage = computed(() => this.currentRoute() === '/');
  readonly isAuthPage = computed(() => this.currentRoute().startsWith('/auth'));

  @HostListener('window:scroll')
  onWindowScroll(): void {
    const scrollY = window.pageYOffset || document.documentElement.scrollTop || 0;
    
    // Update scrolled state for styling
    this.isScrolled.set(scrollY > 50);
    
    // Scroll hide/show behavior
    if (scrollY > this.scrollThreshold) {
      if (scrollY > this.lastScrollY) {
        // Scrolling DOWN - hide navbar
        this.isHidden.set(true);
      } else if (scrollY < this.lastScrollY) {
        // Scrolling UP - show navbar
        this.isHidden.set(false);
      }
    } else {
      // At top - always show navbar
      this.isHidden.set(false);
    }
    
    this.lastScrollY = scrollY <= 0 ? 0 : scrollY;
  }

  scrollToSection(sectionId: string): void {
    const element = document.getElementById(sectionId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  logout(): void {
    this.storeContext.clearStore();
    this.authService.logout();
  }

  goToHome(): void {
    this.storeContext.clearStore();
    this.router.navigate(['/home']);
  }
}
