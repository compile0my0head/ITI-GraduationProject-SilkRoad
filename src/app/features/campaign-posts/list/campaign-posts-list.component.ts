import { Component, inject, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { CampaignPostsService } from '../services/campaign-posts.service';
import { CampaignsService } from '../../campaigns/services/campaigns.service';
import { PlatformsService } from '../../platforms/services/platforms.service';
import { CampaignPost, PostPublishStatus } from '../../../core/models/api.models';

@Component({
  selector: 'app-campaign-posts-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './campaign-posts-list.component.html',
  styleUrls: ['./campaign-posts-list.component.scss']
})
export class CampaignPostsListComponent implements OnInit {
  private postsService = inject(CampaignPostsService);
  private campaignsService = inject(CampaignsService);
  private platformsService = inject(PlatformsService);
  private route = inject(ActivatedRoute);

  // Expose service signals
  posts = this.postsService.posts;
  isLoading = this.postsService.isLoading;
  error = this.postsService.error;

  // Local state
  campaignId = signal<string | null>(null);
  
  // Get campaign name from service
  campaignName = computed(() => {
    const campaign = this.campaignsService.selectedCampaign();
    return campaign?.campaignName || 'Campaign Posts';
  });
  
  // Filter
  selectedStatus = signal<PostPublishStatus | 'all'>('all');

  // Computed filtered posts
  filteredPosts = computed(() => {
    const status = this.selectedStatus();
    if (status === 'all') return this.posts();
    return this.posts().filter(p => p.publishStatus === status);
  });

  // Status counts
  statusCounts = computed(() => {
    const posts = this.posts();
    return {
      all: posts.length,
      Pending: posts.filter(p => p.publishStatus === 'Pending').length,
      Publishing: posts.filter(p => p.publishStatus === 'Publishing').length,
      Published: posts.filter(p => p.publishStatus === 'Published').length,
      Failed: posts.filter(p => p.publishStatus === 'Failed').length
    };
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('campaignId');
    if (id) {
      this.campaignId.set(id);
      // Load posts using Observable for better error handling
      this.postsService.getPostsByCampaign(id).subscribe({
        next: (posts) => {
          console.log(`Loaded ${posts.length} posts for campaign ${id}`);
        },
        error: (err) => {
          console.error('Failed to load campaign posts:', err);
        }
      });
      this.campaignsService.loadCampaign(id);
    }
    this.platformsService.loadConnectedPlatforms();
  }

  setStatusFilter(status: PostPublishStatus | 'all'): void {
    this.selectedStatus.set(status);
  }

  getStatusBadgeClass(status: PostPublishStatus): string {
    const classes: Record<PostPublishStatus, string> = {
      'Pending': 'badge-pending',
      'Publishing': 'badge-publishing',
      'Published': 'badge-published',
      'Failed': 'badge-failed'
    };
    return classes[status] || 'badge-default';
  }

  deletePost(post: CampaignPost): void {
    if (confirm('Are you sure you want to delete this post?')) {
      this.postsService.deletePost(post.id).subscribe({
        next: () => {
          // Reload posts after deletion
          const campaignId = this.campaignId();
          if (campaignId) {
            this.postsService.loadPostsByCampaign(campaignId);
          }
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
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  truncateText(text: string, maxLength: number = 100): string {
    if (text.length <= maxLength) return text;
    return text.slice(0, maxLength) + '...';
  }
}
