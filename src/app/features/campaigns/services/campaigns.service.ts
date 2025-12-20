import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError, finalize, map } from 'rxjs/operators';
import { 
  Campaign, 
  CampaignStage, 
  CampaignPost,
  CreateCampaignRequest,
  UpdateCampaignRequest
} from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CampaignsService {
  private http = inject(HttpClient);

  // State signals
  private readonly _campaigns = signal<Campaign[]>([]);
  private readonly _selectedCampaign = signal<Campaign | null>(null);
  private readonly _campaignPosts = signal<CampaignPost[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // Public computed signals
  readonly campaigns = computed(() => this._campaigns());
  readonly selectedCampaign = computed(() => this._selectedCampaign());
  readonly campaignPosts = computed(() => this._campaignPosts());
  readonly isLoading = computed(() => this._isLoading());
  readonly error = computed(() => this._error());
  readonly totalCampaigns = computed(() => this._campaigns().length);

  // Campaigns by stage
  readonly draftCampaigns = computed(() => 
    this._campaigns().filter(c => c.campaignStage === 'Draft')
  );
  readonly scheduledCampaigns = computed(() => 
    this._campaigns().filter(c => c.campaignStage === 'Scheduled')
  );
  readonly publishedCampaigns = computed(() => 
    this._campaigns().filter(c => c.campaignStage === 'Published')
  );

  /**
   * Load all campaigns for current store
   * GET /api/campaigns
   */
  loadCampaigns(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Campaign[]>(`${environment.apiUrl}/campaigns`).subscribe({
      next: (campaigns) => {
        this._campaigns.set(campaigns);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load campaigns');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get a single campaign by ID
   * GET /api/campaigns/:id
   */
  loadCampaign(id: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<Campaign>(`${environment.apiUrl}/campaigns/${id}`).subscribe({
      next: (campaign) => {
        this._selectedCampaign.set(campaign);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load campaign');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Get campaign by ID (returns Observable)
   * GET /api/campaigns/:id
   */
  getCampaignById(id: string): Observable<Campaign> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<Campaign>(`${environment.apiUrl}/campaigns/${id}`).pipe(
      tap(campaign => {
        this._selectedCampaign.set(campaign);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load campaign');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Create a new campaign (Step 1: Draft)
   * POST /api/campaigns
   */
  createCampaign(data: CreateCampaignRequest): Observable<Campaign> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<Campaign>(`${environment.apiUrl}/campaigns`, data).pipe(
      tap(campaign => {
        this._campaigns.update(campaigns => [campaign, ...campaigns]);
        this._selectedCampaign.set(campaign);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create campaign');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Update a campaign
   * PUT /api/campaigns/:id
   */
  updateCampaign(id: string, data: UpdateCampaignRequest): Observable<Campaign> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<Campaign>(`${environment.apiUrl}/campaigns/${id}`, data).pipe(
      tap(campaign => {
        this._campaigns.update(campaigns => campaigns.map(c => c.id === id ? campaign : c));
        this._selectedCampaign.set(campaign);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update campaign');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Load campaign posts
   * GET /api/campaigns/:id/posts
   */
  loadCampaignPosts(campaignId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<CampaignPost[]>(`${environment.apiUrl}/campaigns/${campaignId}/posts`).subscribe({
      next: (posts) => {
        this._campaignPosts.set(posts);
        this._isLoading.set(false);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to load posts');
        this._isLoading.set(false);
      }
    });
  }

  /**
   * Delete a campaign post
   * DELETE /api/campaign-posts/{postId}
   */
  deletePost(postId: string): Observable<void> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<void>(`${environment.apiUrl}/campaign-posts/${postId}`).pipe(
      tap(() => this._campaignPosts.update(posts => posts.filter(p => p.id !== postId))),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to delete post');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Publish/Activate campaign (Step 3: Publish)
   * PUT /api/campaigns/:id with campaignStage: 'Ready'
   */
  publishCampaign(id: string): Observable<Campaign> {
    return this.updateCampaign(id, { campaignStage: 'Ready' });
  }

  /**
   * Delete a campaign
   * DELETE /api/campaigns/:id
   */
  deleteCampaign(id: string): Observable<void> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<void>(`${environment.apiUrl}/campaigns/${id}`).pipe(
      tap(() => this._campaigns.update(campaigns => campaigns.filter(c => c.id !== id))),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to delete campaign');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Clear selected campaign
   */
  clearSelectedCampaign(): void {
    this._selectedCampaign.set(null);
    this._campaignPosts.set([]);
  }

  /**
   * Clear error state
   */
  clearError(): void {
    this._error.set(null);
  }
}
