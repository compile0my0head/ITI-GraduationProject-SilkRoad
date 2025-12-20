import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CampaignsService } from '../services/campaigns.service';
import { Campaign, CampaignStage } from '../../../core/models/api.models';

@Component({
  selector: 'app-campaigns-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './campaigns-list.component.html',
  styleUrls: ['./campaigns-list.component.scss']
})
export class CampaignsListComponent implements OnInit {
  private campaignsService = inject(CampaignsService);

  // State
  searchQuery = signal('');
  selectedStage = signal<CampaignStage | 'all'>('all');

  // From service
  campaigns = this.campaignsService.campaigns;
  isLoading = this.campaignsService.isLoading;
  error = this.campaignsService.error;

  ngOnInit(): void {
    this.campaignsService.loadCampaigns();
  }

  // Filtered campaigns
  filteredCampaigns = computed(() => {
    let campaigns = this.campaigns();
    const query = this.searchQuery().toLowerCase();
    const stage = this.selectedStage();

    if (query) {
      campaigns = campaigns.filter(c => 
        c.campaignName.toLowerCase().includes(query) ||
        c.campaignDescription?.toLowerCase().includes(query)
      );
    }

    if (stage !== 'all') {
      campaigns = campaigns.filter(c => c.campaignStage === stage);
    }

    return campaigns;
  });

  // Stats
  stats = computed(() => {
    const all = this.campaigns();
    return {
      total: all.length,
      draft: all.filter(c => c.campaignStage === 'Draft').length,
      scheduled: all.filter(c => c.campaignStage === 'Scheduled').length,
      published: all.filter(c => c.campaignStage === 'Published').length
    };
  });

  // Stage options
  stageOptions: { value: CampaignStage | 'all'; label: string }[] = [
    { value: 'all', label: 'All Stages' },
    { value: 'Draft', label: 'Draft' },
    { value: 'Scheduled', label: 'Scheduled' },
    { value: 'Published', label: 'Published' }
  ];

  onSearch(query: string): void {
    this.searchQuery.set(query);
  }

  onStageFilter(stage: CampaignStage | 'all'): void {
    this.selectedStage.set(stage);
  }

  deleteCampaign(campaign: Campaign): void {
    if (confirm(`Are you sure you want to delete "${campaign.campaignName}"?`)) {
      this.campaignsService.deleteCampaign(campaign.id).subscribe();
    }
  }

  getStageClass(stage: CampaignStage): string {
    switch (stage) {
      case 'Draft': return 'stage-draft';
      case 'Scheduled': return 'stage-scheduled';
      case 'Published': return 'stage-published';
      default: return '';
    }
  }

  getStageIcon(stage: CampaignStage): string {
    switch (stage) {
      case 'Draft': return '📝';
      case 'Scheduled': return '📅';
      case 'Published': return '🚀';
      default: return '📋';
    }
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  getDaysRemaining(endDate: string | Date): number {
    const end = new Date(endDate);
    const now = new Date();
    const diff = end.getTime() - now.getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }
}
