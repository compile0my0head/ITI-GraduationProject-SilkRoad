export enum TaskType {
  PostPublish = 'PostPublish',
  ChatResponder = 'ChatResponder'
}

export interface AutomationTask {
  taskID: number;
  storeID: number;
  taskType: TaskType;
  relatedCampaignPostID?: number;
  cronExpression: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateAutomationTaskRequest {
  taskType: TaskType;
  relatedCampaignPostID?: number;
  cronExpression: string;
  isActive: boolean;
}

export interface UpdateAutomationTaskRequest {
  taskType?: TaskType;
  relatedCampaignPostID?: number;
  cronExpression?: string;
  isActive?: boolean;
}
