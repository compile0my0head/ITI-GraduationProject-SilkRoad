import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NavbarStateService {
  // Navbar visibility state (for scroll-based hide/show)
  readonly isHidden = signal(false);
  
  // Sidebar collapsed state
  readonly isCollapsed = signal(false);
  
  setHidden(value: boolean): void {
    this.isHidden.set(value);
  }
  
  setCollapsed(value: boolean): void {
    this.isCollapsed.set(value);
  }
  
  toggleCollapsed(): void {
    this.isCollapsed.update(v => !v);
  }
}
