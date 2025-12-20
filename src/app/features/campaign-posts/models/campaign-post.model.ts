export interface CampaignPost {
  campaignPostID: number;
  campaignID: number;
  postCaption: string;
  postImageUrl?: string;
  scheduledAt?: Date;
  createdAt: Date;
}

export interface CreateCampaignPostRequest {
  campaignID: number;
  postCaption: string;
  postImageUrl?: string;
  scheduledAt?: Date;
}

export interface UpdateCampaignPostRequest {
  postCaption?: string;
  postImageUrl?: string;
  scheduledAt?: Date;
}
