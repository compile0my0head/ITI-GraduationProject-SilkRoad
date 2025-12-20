import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { of, concat, throwError } from 'rxjs';
import { switchMap, tap, catchError, finalize, delay, toArray, timeout } from 'rxjs/operators';
import { CampaignsService } from '../services/campaigns.service';
import { ProductsService } from '../../products/services/products.service';
import { CampaignPostsService } from '../../campaign-posts/services/campaign-posts.service';
import { CampaignStage, CreateCampaignPostRequest, Campaign } from '../../../core/models/api.models';
import { validateMinLength, validateDateRange } from '../../../shared/utils/validators';
import { environment } from '../../../../environments/environment';

// Simple post interface - NO platform data!
// Backend handles platform resolution using storeId from campaign
interface CampaignPost {
  content: string;
  imageUrl?: string;
  scheduledAt?: string;
}

@Component({
  selector: 'app-create-campaign',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './create-campaign.component.html',
  styleUrls: ['./create-campaign.component.scss']
})
export class CreateCampaignComponent implements OnInit {
  private campaignsService = inject(CampaignsService);
  private productsService = inject(ProductsService);
  private postsService = inject(CampaignPostsService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private http = inject(HttpClient);

  // Wizard State
  currentStep = signal(1);
  totalSteps = 3;

  // Edit mode (for draft campaigns)
  isEditMode = signal(false);
  editingCampaignId = signal<string | null>(null);

  // Form Data - Step 1 (Campaign Details)
  campaignName = signal('');
  campaignDescription = signal('');
  goal = signal('');
  targetAudience = signal('');
  bannerUrl = signal('');
  assignedProductId = signal<string | null>(null);

  // Form touched states - Step 1
  campaignNameTouched = signal(false);
  step1SubmitAttempted = signal(false);

  // Form Data - Step 2 (Posts) - NO platform data!
  posts = signal<CampaignPost[]>([]);
  newPostContent = signal('');
  newPostImageUrl = signal('');
  newPostScheduledAt = signal('');
  step2SubmitAttempted = signal(false);

  // AI Caption Generation State
  isGeneratingCaption = signal(false);
  captionGenerationError = signal<string | null>(null);

  // Form Data - Step 3 (Publish/Schedule)
  publishOption = signal<'now' | 'schedule'>('now');
  scheduleStartAt = signal('');
  scheduleEndAt = signal('');
  scheduleStartTouched = signal(false);
  scheduleEndTouched = signal(false);
  step3SubmitAttempted = signal(false);

  // Submission state
  isSubmitting = signal(false);
  submissionProgress = signal('');
  submissionError = signal<string | null>(null);
  retryCountdown = signal(0);
  error = signal<string | null>(null);

  // Products from service
  products = this.productsService.products;
  productsLoading = this.productsService.isLoading;

  // Validation computed - Step 1
  campaignNameValidation = computed(() => {
    const value = this.campaignName();
    if (!value && !this.campaignNameTouched() && !this.step1SubmitAttempted()) {
      return { valid: true, message: '' };
    }
    if (!value || value.trim() === '') {
      return { valid: false, message: 'Campaign name is required' };
    }
    return validateMinLength(value, 3, 'Campaign name');
  });

  // Show error states - Step 1
  showCampaignNameError = computed(() => {
    return (this.campaignNameTouched() || this.step1SubmitAttempted()) && !this.campaignNameValidation().valid;
  });

  // Computed - Step validation
  isStep1Valid = computed(() => {
    return this.campaignNameValidation().valid &&
           this.campaignName().trim().length >= 3;
  });

  isStep2Valid = computed(() => {
    return this.posts().length > 0;
  });

  isStep3Valid = computed(() => {
    if (this.publishOption() === 'now') {
      return true;
    }
    const start = this.scheduleStartAt();
    const end = this.scheduleEndAt();
    if (!start || !end) return false;

    const range = validateDateRange(start, end);
    if (!range.valid) return false;

    const startDate = new Date(start);
    const endDate = new Date(end);
    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) return false;
    return endDate.getTime() > startDate.getTime();
  });

  readonly scheduleRangeValidation = computed(() => {
    if (this.publishOption() !== 'schedule') return { valid: true, message: '' };

    const start = this.scheduleStartAt();
    const end = this.scheduleEndAt();
    if (!start || !end) return { valid: false, message: 'Schedule start and end are required' };

    const range = validateDateRange(start, end);
    if (!range.valid) return range;

    const startDate = new Date(start);
    const endDate = new Date(end);
    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
      return { valid: false, message: 'Please enter valid schedule dates' };
    }
    if (endDate.getTime() <= startDate.getTime()) {
      return { valid: false, message: 'Schedule end must be after schedule start' };
    }

    return { valid: true, message: '' };
  });

  canProceed = computed(() => {
    switch (this.currentStep()) {
      case 1: return this.isStep1Valid();
      case 2: return this.isStep2Valid();
      case 3: return this.isStep3Valid();
      default: return false;
    }
  });

  ngOnInit(): void {
    // Load products for dropdown
    this.productsService.loadProducts();

    // Check if we're editing an existing draft campaign
    const campaignId = this.route.snapshot.queryParamMap.get('editId');
    if (campaignId) {
      this.isEditMode.set(true);
      this.editingCampaignId.set(campaignId);
      this.loadExistingCampaign(campaignId);
    }
  }

  private loadExistingCampaign(campaignId: string): void {
    this.campaignsService.loadCampaign(campaignId);
    
    // Wait for campaign to load and populate form
    const checkInterval = setInterval(() => {
      const campaign = this.campaignsService.selectedCampaign();
      if (campaign && campaign.id === campaignId) {
        clearInterval(checkInterval);
        this.populateFromCampaign(campaign);
      }
    }, 100);

    // Timeout after 10 seconds
    setTimeout(() => clearInterval(checkInterval), 10000);
  }

  private populateFromCampaign(campaign: any): void {
    this.campaignName.set(campaign.campaignName || '');
    this.campaignDescription.set(campaign.campaignDescription || '');
    this.goal.set(campaign.goal || '');
    this.targetAudience.set(campaign.targetAudience || '');
    this.bannerUrl.set(campaign.campaignBannerUrl || '');
    this.assignedProductId.set(campaign.assignedProductId || null);
  }

  // Blur handlers
  onCampaignNameBlur(): void {
    this.campaignNameTouched.set(true);
  }

  onScheduleStartBlur(): void {
    this.scheduleStartTouched.set(true);
  }

  onScheduleEndBlur(): void {
    this.scheduleEndTouched.set(true);
  }

  private toIsoFromLocalDateTime(value: string): string {
    const d = new Date(value);
    if (isNaN(d.getTime())) return '';
    return d.toISOString();
  }

  // Step navigation
  nextStep(): void {
    if (this.currentStep() === 1) {
      this.step1SubmitAttempted.set(true);
    } else if (this.currentStep() === 2) {
      this.step2SubmitAttempted.set(true);
    } else if (this.currentStep() === 3) {
      this.step3SubmitAttempted.set(true);
    }
    
    if (this.currentStep() < this.totalSteps && this.canProceed()) {
      this.currentStep.update(s => s + 1);
    }
  }

  prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  goToStep(step: number): void {
    // Can only go back or to completed steps
    if (step < this.currentStep()) {
      this.currentStep.set(step);
    }
  }

  // Posts management - NO platform data!
  // Backend handles platform resolution using storeId from campaign
  addPost(): void {
    const content = this.newPostContent().trim();
    if (content) {
      this.posts.update(posts => [...posts, {
        content,
        imageUrl: this.newPostImageUrl().trim() || undefined,
        scheduledAt: this.newPostScheduledAt() || undefined
      }]);
      // Reset form
      this.newPostContent.set('');
      this.newPostImageUrl.set('');
      this.newPostScheduledAt.set('');
    }
  }

  removePost(index: number): void {
    this.posts.update(posts => posts.filter((_, i) => i !== index));
  }

  // Submit campaign with proper flow: create campaign -> wait -> create posts
  submitCampaign(): void {
    this.step3SubmitAttempted.set(true);
    
    // Double-check guard to prevent multiple submissions
    if (this.isSubmitting() || !this.isStep3Valid()) return;
    
    this.isSubmitting.set(true);
    this.submissionError.set(null);
    this.submissionProgress.set('Creating campaign...');

    let campaignId: string;
    let createdPosts = 0;
    const postsToCreate = [...this.posts()]; // Copy array to prevent mutation

    // Step 1: Create the campaign (ALWAYS Draft)
    // Set isSchedulingEnabled based on publish option
    this.campaignsService.createCampaign({
      campaignName: this.campaignName(),
      campaignDescription: this.campaignDescription() || undefined,
      campaignStage: 'Draft',
      goal: this.goal() || undefined,
      targetAudience: this.targetAudience() || undefined,
      assignedProductId: this.assignedProductId() || undefined,
      isSchedulingEnabled: this.publishOption() === 'now' || this.publishOption() === 'schedule'
    }).pipe(
      tap(campaign => {
        if (!campaign || !campaign.id) {
          throw new Error('Campaign creation failed - no ID returned');
        }
        campaignId = campaign.id;
        this.submissionProgress.set('Campaign created successfully. Creating posts...');
      }),
      // Step 2: Create posts one by one (sequentially)
      // CRITICAL: One post = One HTTP request. NO platform data sent!
      // Backend handles platform resolution using storeId from campaign
      switchMap(() => {
        if (postsToCreate.length === 0) {
          return of([]);
        }
        
        // Create posts sequentially - one request per post
        const postObservables = postsToCreate.map((post, index) => {
          // Determine scheduled time
          let scheduledTime = post.scheduledAt;
          if (!scheduledTime && this.publishOption() === 'now') {
            scheduledTime = new Date().toISOString();
          }

          // Request body - NO platformIds, NO storeId!
          // Backend resolves platforms using campaign's storeId
          const postRequest: CreateCampaignPostRequest = {
            campaignId: campaignId,
            postCaption: post.content,
            postImageUrl: post.imageUrl || undefined,
            scheduledAt: scheduledTime || undefined
          };

          return of(null).pipe(
            tap(() => this.submissionProgress.set(`Creating post ${index + 1} of ${postsToCreate.length}...`)),
            delay(index > 0 ? 2000 : 0), // Delay before each post (except first)
            switchMap(() => this.postsService.createPost(postRequest)),
            tap(() => createdPosts++),
            catchError(err => {
              console.error(`Failed to create post ${index + 1}:`, err);
              return of(null); // Continue with next post
            })
          );
        });

        // Execute all posts sequentially and wait for completion
        return concat(...postObservables).pipe(toArray());
      }),
      // Step 4: Final action (Publish now OR Schedule campaign)
      switchMap(() => {
        if (this.publishOption() === 'now') {
          this.submissionProgress.set('Finalizing: publishing campaign...');
          return this.campaignsService.updateCampaign(campaignId, { 
            campaignStage: 'Ready',
            isSchedulingEnabled: true
          });
        } else {
          const startIso = this.toIsoFromLocalDateTime(this.scheduleStartAt());
          const endIso = this.toIsoFromLocalDateTime(this.scheduleEndAt());

          if (!startIso || !endIso) {
            return throwError(() => new Error('Invalid schedule start/end date'));
          }

          this.submissionProgress.set('Finalizing: scheduling campaign...');
          return this.campaignsService.updateCampaign(campaignId, {
            campaignStage: 'Scheduled',
            isSchedulingEnabled: true,
            scheduledStartAt: startIso,
            scheduledEndAt: endIso
          });
        }
      }),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe({
      next: () => {
        this.submissionProgress.set(`Campaign created with ${createdPosts} posts!`);
        setTimeout(() => {
          this.router.navigate(['/app/campaigns']);
        }, 1500);
      },
      error: (err: any) => {
        console.error('Campaign submission failed:', err);
        this.submissionError.set(err.message || 'Failed to create campaign. Please try again.');
        this.submissionProgress.set('');
        this.startRetryCountdown(20);
      }
    });
  }

  private startRetryCountdown(seconds: number): void {
    this.retryCountdown.set(seconds);
    const interval = setInterval(() => {
      const current = this.retryCountdown();
      if (current <= 1) {
        clearInterval(interval);
        this.retryCountdown.set(0);
      } else {
        this.retryCountdown.set(current - 1);
      }
    }, 1000);
  }

  // Save as draft
  saveAsDraft(): void {
    if (this.isSubmitting()) return;
    
    this.isSubmitting.set(true);
    this.error.set(null);

    this.campaignsService.createCampaign({
      campaignName: this.campaignName() || 'Untitled Campaign',
      campaignDescription: this.campaignDescription() || undefined,
      campaignStage: 'Draft' as CampaignStage,
      goal: this.goal() || undefined,
      targetAudience: this.targetAudience() || undefined,
      assignedProductId: this.assignedProductId() || undefined,
      isSchedulingEnabled: false
    }).subscribe({
      next: () => {
        this.router.navigate(['/app/campaigns']);
      },
      error: (err: any) => {
        this.error.set(err.message || 'Failed to save draft');
        this.isSubmitting.set(false);
      }
    });
  }

  // Date helpers
  getMinDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  getMinDateTime(): string {
    return new Date().toISOString().slice(0, 16);
  }

  getProductName(productId: string | null): string {
    if (!productId) return 'None';
    const product = this.products().find(p => p.id === productId);
    return product?.productName || 'Unknown Product';
  }

  clearSubmissionError(): void {
    this.submissionError.set(null);
    this.retryCountdown.set(0);
  }

  // AI Caption Generation - Communicates ONLY with n8n webhook
  generateCaption(): void {
    // Build campaign context as human-readable string
    const campaignContext = this.buildCampaignContext();

    // Clear existing caption and lock input
    this.newPostContent.set('');
    this.isGeneratingCaption.set(true);
    this.captionGenerationError.set(null);

    // Send plain text to n8n webhook and expect plain text response
    this.http.post(
      `${environment.n8nWebhookUrl}/generate-caption`,
      campaignContext,
      { 
        headers: { 'Content-Type': 'text/plain' },
        responseType: 'text'
      }
    ).pipe(
      timeout(10000), // 10 second timeout
      catchError(err => {
        this.captionGenerationError.set(
          err.name === 'TimeoutError' 
            ? 'Caption generation timed out. Please try again.'
            : 'Failed to generate caption. Please try again or write manually.'
        );
        return throwError(() => err);
      }),
      finalize(() => this.isGeneratingCaption.set(false))
    ).subscribe({
      next: (response) => {
        if (response && typeof response === 'string' && response.trim()) {
          this.newPostContent.set(response.trim());
          this.captionGenerationError.set(null);
        } else {
          this.captionGenerationError.set('Invalid response received. Please try again.');
        }
      },
      error: () => {
        // Error already handled in catchError
      }
    });
  }

  // Build campaign context as formatted text string (NOT structured data)
  private buildCampaignContext(): string {
    const parts: string[] = [];

    if (this.campaignName()) {
      parts.push(`Campaign name: ${this.campaignName()}.`);
    }

    if (this.goal()) {
      parts.push(`Campaign goal: ${this.goal()}.`);
    }

    if (this.targetAudience()) {
      parts.push(`Target audience: ${this.targetAudience()}.`);
    }

    // Get assigned product name if available
    if (this.assignedProductId()) {
      const product = this.products().find(p => p.id === this.assignedProductId());
      if (product) {
        parts.push(`Assigned product: ${product.productName}.`);
      }
    }

    return parts.join('\n');
  }
}
