import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CampaignsService } from '../services/campaigns.service';
import { Campaign, CampaignStage } from '../../../core/models/api.models';
import { validateMinLength, validateFutureDate, validateDateRange } from '../../../shared/utils/validators';

@Component({
  selector: 'app-edit-campaign',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './edit-campaign.component.html',
  styleUrls: ['./edit-campaign.component.scss']
})
export class EditCampaignComponent implements OnInit {
  private campaignsService = inject(CampaignsService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // Local state
  campaign = signal<Campaign | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  isSaving = signal(false);
  isPublishing = signal(false);
  activeTab = signal<'details' | 'posts' | 'settings'>('details');

  // Form signals
  campaignName = signal('');
  campaignDescription = signal('');
  goal = signal('');
  targetAudience = signal('');
  scheduledStartAt = signal('');
  scheduledEndAt = signal('');
  isSchedulingEnabled = signal(false);

  // Form touched states
  campaignNameTouched = signal(false);
  scheduledStartAtTouched = signal(false);
  scheduledEndAtTouched = signal(false);
  saveAttempted = signal(false);

  // Validation computed
  campaignNameValidation = computed(() => {
    const value = this.campaignName();
    if (!value && !this.campaignNameTouched() && !this.saveAttempted()) {
      return { valid: true, message: '' };
    }
    if (!value || value.trim() === '') {
      return { valid: false, message: 'Campaign name is required' };
    }
    return validateMinLength(value, 3, 'Campaign name');
  });

  startDateValidation = computed(() => {
    const value = this.scheduledStartAt();
    if (!value) return { valid: true, message: '' }; // Optional field
    return validateFutureDate(value, 'Start date');
  });

  endDateValidation = computed(() => {
    const value = this.scheduledEndAt();
    if (!value) return { valid: true, message: '' }; // Optional field
    const dateCheck = validateFutureDate(value, 'End date');
    if (!dateCheck.valid) return dateCheck;
    if (this.scheduledStartAt()) {
      return validateDateRange(this.scheduledStartAt(), value);
    }
    return { valid: true, message: '' };
  });

  // Show error states
  showCampaignNameError = computed(() => {
    return (this.campaignNameTouched() || this.saveAttempted()) && !this.campaignNameValidation().valid;
  });

  showStartDateError = computed(() => {
    return (this.scheduledStartAtTouched() || this.saveAttempted()) && !this.startDateValidation().valid;
  });

  showEndDateError = computed(() => {
    return (this.scheduledEndAtTouched() || this.saveAttempted()) && !this.endDateValidation().valid;
  });

  // Blur handlers
  onCampaignNameBlur(): void {
    this.campaignNameTouched.set(true);
  }

  onStartDateBlur(): void {
    this.scheduledStartAtTouched.set(true);
  }

  onEndDateBlur(): void {
    this.scheduledEndAtTouched.set(true);
  }

  // Computed
  canEdit = computed(() => {
    const campaign = this.campaign();
    return campaign && campaign.campaignStage === 'Draft';
  });

  hasChanges = computed(() => {
    const campaign = this.campaign();
    if (!campaign) return false;

    return (
      this.campaignName() !== campaign.campaignName ||
      this.campaignDescription() !== (campaign.campaignDescription || '') ||
      this.goal() !== (campaign.goal || '') ||
      this.targetAudience() !== (campaign.targetAudience || '')
    );
  });

  isFormValid = computed(() => {
    return this.campaignNameValidation().valid &&
           this.startDateValidation().valid &&
           this.endDateValidation().valid;
  });

  // Campaign scheduling status
  campaignSchedulingStatus = computed(() => {
    return this.isSchedulingEnabled() ? 'Enabled' : 'Disabled';
  });

  isTogglingScheduling = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.campaignsService.getCampaignById(id).subscribe({
        next: (campaign) => {
          this.campaign.set(campaign);
          this.populateForm(campaign);
        },
        error: (err) => {
          console.error('Failed to load campaign:', err);
          this.error.set(err.error?.message || 'Failed to load campaign');
        }
      });
    } else {
      this.error.set('No campaign ID provided');
    }
  }

  private populateForm(campaign: Campaign): void {
    this.campaignName.set(campaign.campaignName);
    this.campaignDescription.set(campaign.campaignDescription || '');
    this.goal.set(campaign.goal || '');
    this.targetAudience.set(campaign.targetAudience || '');
    this.isSchedulingEnabled.set(campaign.isSchedulingEnabled ?? false);
    if (campaign.scheduledStartAt) {
      this.scheduledStartAt.set(this.formatDateForInput(campaign.scheduledStartAt));
    }
    if (campaign.scheduledEndAt) {
      this.scheduledEndAt.set(this.formatDateForInput(campaign.scheduledEndAt));
    }
  }

  private formatDateForInput(date: string): string {
    return new Date(date).toISOString().slice(0, 16);
  }

  formatDate(date: string | undefined): string {
    if (!date) return 'Not set';
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  setActiveTab(tab: 'details' | 'posts' | 'settings'): void {
    this.activeTab.set(tab);
  }

  // Save changes
  saveChanges(): void {
    this.saveAttempted.set(true);
    
    const campaign = this.campaign();
    if (!campaign || this.isSaving() || !this.isFormValid()) return;

    this.isSaving.set(true);

    this.campaignsService.updateCampaign(campaign.id, {
      campaignName: this.campaignName(),
      campaignDescription: this.campaignDescription(),
      goal: this.goal(),
      targetAudience: this.targetAudience(),
      scheduledStartAt: this.scheduledStartAt() || undefined,
      scheduledEndAt: this.scheduledEndAt() || undefined,
      isSchedulingEnabled: this.isSchedulingEnabled()
    }).subscribe({
      next: () => {
        // Reset touched states on successful save
        this.saveAttempted.set(false);
        this.campaignNameTouched.set(false);
        this.scheduledStartAtTouched.set(false);
        this.scheduledEndAtTouched.set(false);
        // Reload campaign
        this.campaignsService.loadCampaign(campaign.id);
        this.isSaving.set(false);
      },
      error: (err: any) => {
        console.error('Failed to save changes', err);
        this.isSaving.set(false);
      }
    });
  }

  // Stage actions
  publishCampaign(): void {
    const campaign = this.campaign();
    if (!campaign || this.isPublishing() || this.isSaving()) return;

    // Prevent multiple rapid clicks
    this.isPublishing.set(true);

    // Use the proper publishCampaign method that sets stage to 'Ready'
    this.campaignsService.publishCampaign(campaign.id).subscribe({
      next: () => {
        console.log('Campaign published successfully');
        // Reload campaign to get updated stage
        this.campaignsService.loadCampaign(campaign.id);
        this.isPublishing.set(false);
        // Navigate back to campaigns list
        this.router.navigate(['/app/campaigns']);
      },
      error: (err: any) => {
        console.error('Failed to publish campaign', err);
        this.error.set(err.error?.message || 'Failed to publish campaign');
        this.isPublishing.set(false);
      }
    });
  }

  updateStage(newStage: CampaignStage): void {
    const campaign = this.campaign();
    if (!campaign || this.isSaving()) return;

    this.isSaving.set(true);
    this.campaignsService.updateCampaign(campaign.id, {
      campaignStage: newStage
    }).subscribe({
      next: () => {
        this.campaignsService.loadCampaign(campaign.id);
        this.isSaving.set(false);
      },
      error: (err: any) => {
        console.error('Failed to update stage', err);
        this.isSaving.set(false);
      }
    });
  }

  getStageClass(stage: CampaignStage): string {
    switch (stage) {
      case 'Draft': return 'stage-draft';
      case 'InReview': return 'stage-review';
      case 'Scheduled': return 'stage-scheduled';
      case 'Ready': return 'stage-ready';
      case 'Published': return 'stage-published';
      default: return '';
    }
  }

  getStageIcon(stage: CampaignStage): string {
    switch (stage) {
      case 'Draft': return '📝';
      case 'InReview': return '👀';
      case 'Scheduled': return '📅';
      case 'Ready': return '✅';
      case 'Published': return '🚀';
      default: return '📋';
    }
  }

  goBack(): void {
    this.router.navigate(['/app/campaigns']);
  }

  // Toggle campaign scheduling
  toggleCampaignScheduling(): void {
    const campaign = this.campaign();
    if (!campaign || this.isTogglingScheduling() || this.isSaving()) return;

    const newSchedulingState = !this.isSchedulingEnabled();
    this.isTogglingScheduling.set(true);
    this.error.set(null);

    // Send PUT request with all campaign data, only changing isSchedulingEnabled
    this.campaignsService.updateCampaign(campaign.id, {
      campaignName: this.campaignName(),
      campaignDescription: this.campaignDescription() || undefined,
      campaignStage: campaign.campaignStage,
      goal: this.goal() || undefined,
      targetAudience: this.targetAudience() || undefined,
      scheduledStartAt: this.scheduledStartAt() || undefined,
      scheduledEndAt: this.scheduledEndAt() || undefined,
      isSchedulingEnabled: newSchedulingState,
      assignedProductId: campaign.assignedProductId || undefined
    }).subscribe({
      next: () => {
        // Update local state
        this.isSchedulingEnabled.set(newSchedulingState);
        // Reload campaign to get updated data from server
        this.campaignsService.loadCampaign(campaign.id);
        this.isTogglingScheduling.set(false);
      },
      error: (err: any) => {
        console.error('Failed to toggle campaign scheduling', err);
        this.error.set(err.error?.message || 'Failed to update campaign scheduling');
        this.isTogglingScheduling.set(false);
      }
    });
  }
}
