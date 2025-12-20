import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, catchError, finalize, map } from 'rxjs/operators';
import { CampaignPost, CreateCampaignPostRequest } from '../../../core/models/api.models';
import { environment } from '../../../../environments/environment';

export interface UpdateCampaignPostRequest {
  postCaption?: string;
  postImageUrl?: string;
  scheduledAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CampaignPostsService {
  private http = inject(HttpClient);
  private readonly API_URL = environment.apiUrl;

  // State signals
  private _posts = signal<CampaignPost[]>([]);
  private _selectedPost = signal<CampaignPost | null>(null);
  private _isLoading = signal(false);
  private _error = signal<string | null>(null);

  // Public readonly signals
  readonly posts = this._posts.asReadonly();
  readonly selectedPost = this._selectedPost.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  /**
   * Load all campaign posts for current store
   * GET /api/campaign-posts
   */
  loadAllPosts(): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<CampaignPost[]>(`${this.API_URL}/campaign-posts`)
      .pipe(
        tap(posts => this._posts.set(posts)),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load campaign posts');
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Load posts for a specific campaign
   * GET /api/campaign-posts?campaignId={campaignId} or filter client-side
   */
  loadPostsByCampaign(campaignId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    // Try campaign-specific endpoint first, fallback to all posts with filter
    this.http.get<CampaignPost[] | { posts: CampaignPost[] }>(`${this.API_URL}/campaign-posts`)
      .pipe(
        tap(response => {
          // Handle both array and object responses
          const posts = Array.isArray(response) ? response : (response.posts || []);
          const filtered = posts.filter(p => p.campaignId === campaignId);
          this._posts.set(filtered);
        }),
        catchError(err => {
          console.error('Failed to load campaign posts:', err);
          this._error.set(err.error?.message || 'Failed to load campaign posts');
          this._posts.set([]);
          return of([]);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Get posts by campaign ID (returns Observable for reactive usage)
   * GET /api/campaign-posts
   */
  getPostsByCampaign(campaignId: string): Observable<CampaignPost[]> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<CampaignPost[] | { posts: CampaignPost[] }>(`${this.API_URL}/campaign-posts`).pipe(
      map(response => {
        const posts = Array.isArray(response) ? response : (response.posts || []);
        return posts.filter(p => p.campaignId === campaignId);
      }),
      tap(filtered => this._posts.set(filtered)),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load campaign posts');
        return of([]);
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Get a single post by ID
   * GET /api/campaign-posts/{postId}
   */
  loadPost(postId: string): void {
    this._isLoading.set(true);
    this._error.set(null);

    this.http.get<CampaignPost>(`${this.API_URL}/campaign-posts/${postId}`)
      .pipe(
        tap(post => this._selectedPost.set(post)),
        catchError(err => {
          this._error.set(err.error?.message || 'Failed to load post');
          return of(null);
        }),
        finalize(() => this._isLoading.set(false))
      )
      .subscribe();
  }

  /**
   * Get post by ID (returns Observable)
   * GET /api/campaign-posts/{postId}
   */
  getPostById(postId: string): Observable<CampaignPost> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<CampaignPost>(`${this.API_URL}/campaign-posts/${postId}`).pipe(
      tap(post => this._selectedPost.set(post)),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load post');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Create a new campaign post
   * POST /api/campaign-posts
   */
  createPost(request: CreateCampaignPostRequest): Observable<CampaignPost> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<CampaignPost>(`${this.API_URL}/campaign-posts`, request).pipe(
      tap(post => this._posts.update(posts => [...posts, post])),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create post');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Update a campaign post
   * PUT /api/campaign-posts/{postId}
   */
  updatePost(postId: string, updates: UpdateCampaignPostRequest): Observable<CampaignPost> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.put<CampaignPost>(`${this.API_URL}/campaign-posts/${postId}`, updates).pipe(
      tap(updated => {
        this._posts.update(posts => posts.map(p => p.id === postId ? updated : p));
        this._selectedPost.set(updated);
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to update post');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  /**
   * Delete a campaign post
   * DELETE /api/campaign-posts/{postId}
   */
  deletePost(postId: string): Observable<void> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<void>(`${this.API_URL}/campaign-posts/${postId}`).pipe(
      tap(() => {
        this._posts.update(posts => posts.filter(p => p.id !== postId));
      }),
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to delete post');
        throw err;
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  clearSelectedPost(): void {
    this._selectedPost.set(null);
  }

  clearError(): void {
    this._error.set(null);
  }
}
