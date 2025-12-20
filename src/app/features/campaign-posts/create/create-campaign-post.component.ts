import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CampaignPostsService } from '../services/campaign-posts.service';

/**
 * Create Campaign Post Component
 * 
 * RESPONSIBILITY: Create a single campaign post
 * 
 * CRITICAL RULES:
 * - NO platform data is sent to the API
 * - One post = One HTTP request
 * - Backend handles platform resolution using storeId from campaign
 * - Request body: campaignId, postCaption, postImageUrl (optional), scheduledAt (optional)
 */
@Component({
  selector: 'app-create-campaign-post',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-campaign-post.component.html',
  styleUrls: ['./create-campaign-post.component.scss']
})
export class CreateCampaignPostComponent implements OnInit {
  private postsService = inject(CampaignPostsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // State
  isLoading = signal(false);
  error = signal<string | null>(null);

  // Route params
  campaignId = signal<string | null>(null);

  // Form signals - NO platform selection!
  postCaption = signal('');
  postImageUrl = signal('');
  scheduledAt = signal('');

  // Preview mode
  showPreview = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('campaignId');
    this.campaignId.set(id);
    
    if (!id) {
      this.error.set('No campaign ID provided');
    }
  }

  togglePreview(): void {
    this.showPreview.update(v => !v);
  }

  /**
   * Submit post creation
   * 
   * CRITICAL: 
   * - One request per post
   * - NO platformIds in request
   * - Backend resolves platforms using campaign's storeId
   */
  onSubmit(): void {
    const campaignId = this.campaignId();
    const postCaption = this.postCaption().trim();
    
    if (!postCaption || !campaignId) {
      this.error.set('Post caption is required');
      return;
    }

    // Prevent duplicate submissions
    if (this.isLoading()) return;

    this.isLoading.set(true);
    this.error.set(null);

    // If no scheduled time is set, use current date/time for immediate publishing
    const scheduledAt = this.scheduledAt() || new Date().toISOString();

    // Request body - NO platformIds!
    // Backend handles platform resolution using storeId from campaign
    this.postsService.createPost({
      campaignId: campaignId,
      postCaption: postCaption,
      postImageUrl: this.postImageUrl().trim() || undefined,
      scheduledAt: scheduledAt
    }).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/app/campaigns', campaignId, 'posts']);
      },
      error: (err) => {
        console.error('Failed to create post:', err);
        this.error.set(err.error?.message || 'Failed to create post');
        this.isLoading.set(false);
      }
    });
  }

  clearError(): void {
    this.error.set(null);
    this.postsService.clearError();
  }
}
