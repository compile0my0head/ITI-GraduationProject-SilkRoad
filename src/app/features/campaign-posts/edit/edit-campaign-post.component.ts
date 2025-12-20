import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CampaignPostsService } from '../services/campaign-posts.service';
import { PlatformsService } from '../../platforms/services/platforms.service';
import { CampaignPost } from '../../../core/models/api.models';

@Component({
  selector: 'app-edit-campaign-post',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './edit-campaign-post.component.html',
  styleUrls: ['./edit-campaign-post.component.scss']
})
export class EditCampaignPostComponent implements OnInit {
  private postsService = inject(CampaignPostsService);
  private platformsService = inject(PlatformsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // State
  selectedPost = signal<CampaignPost | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  connectedPlatforms = this.platformsService.connectedPlatforms;

  // Route params
  campaignId = signal<string | null>(null);
  postId = signal<string | null>(null);

  // Form signals
  postCaption = signal('');
  postImageUrl = signal('');
  scheduledAt = signal('');

  // Computed
  isPostPublished = computed(() => {
    const post = this.selectedPost();
    return post?.publishStatus === 'Published';
  });

  ngOnInit(): void {
    const campaignId = this.route.snapshot.paramMap.get('campaignId');
    const postId = this.route.snapshot.paramMap.get('postId');
    
    this.campaignId.set(campaignId);
    this.postId.set(postId);

    if (postId) {
      this.loadPost(postId);
    }
    this.platformsService.loadConnectedPlatforms();
  }

  private loadPost(postId: string): void {
    this.isLoading.set(true);
    this.postsService.getPostById(postId).subscribe({
      next: (post) => {
        this.selectedPost.set(post);
        this.postCaption.set(post.postCaption);
        this.postImageUrl.set(post.postImageUrl || '');
        // Format scheduledAt for datetime-local input
        if (post.scheduledAt) {
          this.scheduledAt.set(this.formatDateForInput(post.scheduledAt));
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load post:', err);
        this.error.set(err.error?.message || 'Failed to load post');
        this.isLoading.set(false);
      }
    });
  }

  onSubmit(): void {
    if (!this.postCaption() || !this.postId()) {
      this.error.set('Post caption is required');
      return;
    }

    // Prevent editing published posts
    if (this.isPostPublished()) {
      this.error.set('Cannot edit a post that has already been published');
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    // Convert scheduledAt from local datetime to ISO if provided
    let scheduledTime: string | undefined = undefined;
    if (this.scheduledAt()) {
      try {
        scheduledTime = new Date(this.scheduledAt()).toISOString();
      } catch (err) {
        console.error('Invalid date format:', this.scheduledAt());
      }
    }

    this.postsService.updatePost(this.postId()!, {
      postCaption: this.postCaption(),
      postImageUrl: this.postImageUrl() || undefined,
      scheduledAt: scheduledTime
    }).subscribe({
      next: () => {
        this.isLoading.set(false);
        if (this.campaignId()) {
          this.router.navigate(['/app/campaigns', this.campaignId(), 'posts']);
        }
      },
      error: (err) => {
        console.error('Failed to update post:', err);
        this.error.set(err.error?.message || 'Failed to update post');
        this.isLoading.set(false);
      }
    });
  }

  clearError(): void {
    this.error.set(null);
    this.postsService.clearError();
  }

  formatDateForInput(dateStr: string | undefined): string {
    if (!dateStr) return '';
    try {
      const date = new Date(dateStr);
      if (isNaN(date.getTime())) return '';
      // Convert to local datetime-local format (YYYY-MM-DDTHH:mm)
      return date.toISOString().slice(0, 16);
    } catch (err) {
      console.error('Error formatting date:', err);
      return '';
    }
  }
}
