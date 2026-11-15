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
