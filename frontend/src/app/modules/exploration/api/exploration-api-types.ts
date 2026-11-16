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

export interface LocationDto {
  latitude: number;
  longitude: number;
}

export interface KeyPointDto {
  id: string;
  name: string;
  description: string;
  imageId: string;
  imageUrl: string;
  secret: string | null;
  isPublic: boolean;
  order: number;
  location: LocationDto;
}

export interface TourWithKeyPointsDto {
  id: string;
  name: string;
  status: TourStatus;
  lengthInKm: number | null;
  keyPoints: KeyPointDto[];
}

export interface AddKeyPointDto {
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  isPublic: boolean;
  location: LocationDto;
}

export interface ChangeKeyPointDto {
  name: string;
  description: string;
  imageId: string;
  secret: string | null;
  isPublic: boolean;
  location: LocationDto;
}

export interface TourLengthDto {
  lengthInKm: number;
}

export interface UploadedImageDto {
  id: string;
  url: string;
}

export type TourSortOrder = 'Newest' | 'DifficultyAscending' | 'DifficultyDescending';

export interface TourSearchCriteriaDto {
  name: string | null;
  difficulties: TourDifficulty[] | null;
  tags: string[] | null;
  transports: TransportMode[] | null;
  sort: TourSortOrder;
}

export interface TouristPositionDto {
  latitude: number;
  longitude: number;
  markedAt: string;
}
export interface PublicKeyPointDto {
  id: string;
  name: string;
  description: string;
  imageUrl: string;
  location: LocationDto;
}

export interface TourPreviewDto {
  id: string;
  name: string;
  description: string;
  difficulty: TourDifficulty;
  tags: string[];
  lengthInKm: number | null;
  transportTimes: TransportTimeDto[];
  publicKeyPoints: PublicKeyPointDto[];
}
