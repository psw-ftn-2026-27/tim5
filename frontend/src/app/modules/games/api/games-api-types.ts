export type ChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

export interface TimeWindowDto {
  start: string;
  end: string;
}

export interface SocialChallengeDto {
  id: string;
  name: string;
  exitMessage: string;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  window: TimeWindowDto;
  status: ChallengeStatus;
}

export interface SocialChallengeInputDto {
  name: string;
  exitMessage: string;
  requiredPeople: number;
  rangeInMeters: number;
  xp: number;
  window: TimeWindowDto;
}
