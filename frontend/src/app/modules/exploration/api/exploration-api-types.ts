export type TourDifficulty = 'Easy' | 'Moderate' | 'Hard';

export type TourStatus = 'Draft' | 'Published' | 'Archived';

export type TransportMode = 'Walking' | 'Bicycle' | 'Car';

export interface TransportTimeDto {
  transport: TransportMode;
  minutes: number;
}

export interface GroupSizeDto {
  min: number;
  max: number;
}

export interface PriceDto {
  isFree: boolean;
  amount: number;
}

export interface TourDto {
  id: string;
  authorId: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  status: TourStatus;
  publishedAt: string | null;
  archivedAt: string | null;
  recommendedGroupSize: GroupSizeDto | null;
  transportTimes: TransportTimeDto[];
}

export interface CreateTourDto {
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  recommendedGroupSize: GroupSizeDto;
}

export type TourSortOrder = 'Newest' | 'DifficultyAscending' | 'DifficultyDescending';

export interface TourSearchCriteriaDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transports: TransportMode[] | null;
  sort: TourSortOrder;
}
