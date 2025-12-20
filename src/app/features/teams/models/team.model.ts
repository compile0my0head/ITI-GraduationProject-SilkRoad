export enum TeamMemberRole {
  Owner = 'Owner',
  Moderator = 'Moderator',
  Member = 'Member'
}

export interface Team {
  teamID: number;
  storeID: number;
  teamName: string;
  createdAt: Date;
}

export interface TeamMember {
  teamID: number;
  userID: number;
  role: TeamMemberRole;
  addedAt: Date;
}

export interface TeamWithMembers extends Team {
  members: TeamMember[];
}

export interface CreateTeamRequest {
  teamName: string;
}

export interface AddTeamMemberRequest {
  userID: number;
  role: TeamMemberRole;
}

export interface UpdateTeamMemberRequest {
  role: TeamMemberRole;
}
