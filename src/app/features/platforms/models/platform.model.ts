export enum PlatformName {
  Facebook = 'Facebook',
  Instagram = 'Instagram',
  Twitter = 'Twitter',
  LinkedIn = 'LinkedIn',
  TikTok = 'TikTok'
}

export interface SocialPlatform {
  platformID: number;
  storeID: number;
  platformName: PlatformName;
  externalPageID: string;
  pageName: string;
  accessToken: string;
  connectedAt: Date;
  updatedAt: Date;
}

export interface ConnectPlatformRequest {
  platformName: PlatformName;
  externalPageID: string;
  pageName: string;
  accessToken: string;
}

export interface UpdatePlatformRequest {
  pageName?: string;
  accessToken?: string;
}
