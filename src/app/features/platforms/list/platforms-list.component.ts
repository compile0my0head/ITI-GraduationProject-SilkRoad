import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PlatformsService, AvailablePlatform } from '../services/platforms.service';
import { SocialPlatform } from '../../../core/models/api.models';

@Component({
  selector: 'app-platforms-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './platforms-list.component.html',
  styleUrls: ['./platforms-list.component.scss']
})
export class PlatformsListComponent implements OnInit {
  private platformsService = inject(PlatformsService);

  // Expose service signals
  platforms = this.platformsService.platforms;
  availablePlatforms = this.platformsService.availablePlatforms;
  isLoading = this.platformsService.isLoading;
  error = this.platformsService.error;

  // Local state
  showConnectModal = signal(false);
  selectedPlatform = signal<AvailablePlatform | null>(null);

  ngOnInit(): void {
    this.platformsService.loadConnectedPlatforms();
    this.platformsService.loadAvailablePlatforms();
  }

  getPlatformIcon(platformName: string): string {
    const icons: Record<string, string> = {
      'Facebook': '📘',
      'Instagram': '📷',
      'TikTok': '🎵',
      'YouTube': '📺'
    };
    return icons[platformName] || '🔗';
  }

  getPlatformColor(platformName: string): string {
    const colors: Record<string, string> = {
      'Facebook': '#1877F2',
      'Instagram': '#E4405F',
      'TikTok': '#000000',
      'YouTube': '#FF0000'
    };
    return colors[platformName] || '#6366f1';
  }

  openConnectModal(platform: AvailablePlatform): void {
    this.selectedPlatform.set(platform);
    this.showConnectModal.set(true);
  }

  closeConnectModal(): void {
    this.showConnectModal.set(false);
    this.selectedPlatform.set(null);
    this.platformsService.clearError();
  }

  isConnected(platformName: string): boolean {
    return this.platforms().some(p => p.platformName === platformName && p.isConnected);
  }

  getConnectedPlatform(platformName: string): SocialPlatform | undefined {
    return this.platforms().find(p => p.platformName === platformName);
  }

  disconnectPlatform(platform: SocialPlatform): void {
    if (confirm(`Are you sure you want to disconnect ${platform.platformName}?`)) {
      this.platformsService.disconnectPlatform(platform.id).subscribe({
        next: () => {
          // Platform disconnected successfully
        },
        error: () => {
          // Error is handled by service
        }
      });
    }
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
