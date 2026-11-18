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

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface AnswerDto {
  text: string;
  isCorrect: boolean;
}

export interface QuestionDto {
  text: string;
  answers: AnswerDto[];
}

export interface QuizSummaryDto {
  id: string;
  name: string;
  xp: number;
  questionCount: number;
  status: ChallengeStatus;
}

export interface QuizDto {
  id: string;
  name: string;
  instructions: string;
  xp: number;
  location: LocationDto;
  questions: QuestionDto[];
  status: ChallengeStatus;
}

export interface QuizInputDto {
  name: string;
  instructions: string;
  xp: number;
  location: LocationDto;
  questions: QuestionDto[];
}

export interface HiddenPointDto {
  id: string;
  imageId: string;
  imageUrl: string;
  location: LocationDto;
}

export interface LocationChallengeSummaryDto {
  id: string;
  name: string;
  xp: number;
  hiddenPointCount: number;
  status: ChallengeStatus;
}

export interface LocationChallengeDto {
  id: string;
  name: string;
  xp: number;
  status: ChallengeStatus;
  startLocation: LocationDto;
  hiddenPoints: HiddenPointDto[];
}

export interface LocationChallengeInputDto {
  name: string;
  xp: number;
  startLocation: LocationDto;
}

export interface HiddenPointInputDto {
  imageId: string;
  location: LocationDto;
}

export interface UploadedImageDto {
  id: string;
  url: string;
}

export type ChallengeKind = 'Quiz' | 'SocialChallenge' | 'LocationChallenge';

export interface ChallengeOverviewItemDto {
  id: string;
  name: string;
  kind: ChallengeKind;
  xp: number;
  status: ChallengeStatus;
}

export interface ChallengeOverviewFilterDto {
  status: ChallengeStatus | null;
}

export interface NearbyLocationChallengeDto {
  id: string;
  name: string;
  xp: number;
  hiddenPointCount: number;
  startLocation: LocationDto;
  distanceInMeters: number;
  isActivated: boolean;
}

export interface NearbyLocationChallengesDto {
  position: LocationDto | null;
  challenges: NearbyLocationChallengeDto[];
}

export interface ActiveLocationChallengeDto {
  challengeId: string;
  name: string;
  center: LocationDto;
  radiusInMeters: number;
  photoUrls: string[];
}
