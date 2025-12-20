import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { PlatformsService } from '../services/platforms.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-connect-platform',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './connect-platform.component.html',
  styleUrls: ['./connect-platform.component.scss']
})
export class ConnectPlatformComponent implements OnInit {
  private platformsService = inject(PlatformsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // Expose service signals
  isLoading = this.platformsService.isLoading;
  error = this.platformsService.error;

  // Route param
  platformName = signal('');
  
  // OAuth code from URL (after redirect back from Facebook)
  oauthCode = signal('');

  // Connection step
  connectionStep = signal<'info' | 'connecting' | 'success'>('info');

  ngOnInit(): void {
    const platform = this.route.snapshot.paramMap.get('platform');
    this.platformName.set(platform || 'facebook');

    // Check for OAuth callback code in query params
    const code = this.route.snapshot.queryParamMap.get('code');
    if (code) {
      this.oauthCode.set(code);
      this.handleOAuthCallback(code);
    }
  }

  getPlatformDisplayName(): string {
    const names: Record<string, string> = {
      'facebook': 'Facebook',
      'instagram': 'Instagram',
      'tiktok': 'TikTok',
      'youtube': 'YouTube'
    };
    return names[this.platformName().toLowerCase()] || this.platformName();
  }

  getPlatformIcon(): string {
    const icons: Record<string, string> = {
      'facebook': '📘',
      'instagram': '📷',
      'tiktok': '🎵',
      'youtube': '📺'
    };
    return icons[this.platformName().toLowerCase()] || '🔗';
  }

  getPlatformColor(): string {
    const colors: Record<string, string> = {
      'facebook': '#1877F2',
      'instagram': '#E4405F',
      'tiktok': '#000000',
      'youtube': '#FF0000'
    };
    return colors[this.platformName().toLowerCase()] || '#6366f1';
  }

  getRedirectUri(): string {
    // The OAuth redirect URI - must match what's configured in Facebook App
    return `${window.location.origin}/app/platforms/connect/${this.platformName()}`;
  }

  startConnection(): void {
    // In a real implementation, this would redirect to Facebook OAuth
    // For now, show a message that OAuth needs to be configured
    this.connectionStep.set('connecting');
    
    // The actual Facebook OAuth URL would look like:
    // https://www.facebook.com/v18.0/dialog/oauth?
    //   client_id={app-id}&
    //   redirect_uri={redirect-uri}&
    //   scope=pages_manage_posts,pages_read_engagement
    
    // For demo, we'll show a message
    alert('Facebook OAuth integration requires a Facebook App ID to be configured. The OAuth flow will redirect you to Facebook to authorize the connection.');
  }

  handleOAuthCallback(code: string): void {
    this.connectionStep.set('connecting');
    
    this.platformsService.connectFacebook({
      code,
      redirectUri: this.getRedirectUri()
    }).subscribe({
      next: () => {
        this.connectionStep.set('success');
      },
      error: () => {
        // Error handled by service
      }
    });
  }

  finishConnection(): void {
    this.router.navigate(['/app/platforms']);
  }

  clearError(): void {
    this.platformsService.clearError();
  }
}
