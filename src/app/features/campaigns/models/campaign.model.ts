export enum CampaignState {
  Draft = 'Draft',
  InReview = 'InReview',
  Scheduled = 'Scheduled',
  Ready = 'Ready',
  Published = 'Published'
}

export interface Campaign {
  campaignID: number;
  storeID: number;
  campaignName: string;
  campaignBannerUrl: string;
  assignedProductID: number;
  currentStage: number;
  state: CampaignState;
  goal?: string;
  targetAudience?: any;
  createdByUserID: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateCampaignRequest {
  campaignName: string;
  campaignBannerUrl: string;
  assignedProductID: number;
  goal?: string;
  targetAudience?: any;
}

export interface UpdateCampaignRequest {
  campaignName?: string;
  campaignBannerUrl?: string;
  assignedProductID?: number;
  currentStage?: number;
  state?: CampaignState;
  goal?: string;
  targetAudience?: any;
}
