import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from '../navbar/navbar.component';
import { NavbarStateService } from '../../services/navbar-state.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, NavbarComponent],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  private navbarState = inject(NavbarStateService);
  
  // Expose navbar state for template binding
  readonly sidebarCollapsed = this.navbarState.isCollapsed;
  readonly sidebarHidden = this.navbarState.isHidden;
}
