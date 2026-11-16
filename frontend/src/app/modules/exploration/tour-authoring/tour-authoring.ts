import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  AddKeyPointDto,
  ChangeKeyPointDto,
  CreateTourDto,
  GroupSizeDto,
  PriceDto,
  TourDto,
  TourLengthDto,
  TransportTimeDto,
  UploadedImageDto,
} from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';
const IMAGES_URL = '/api/exploration/images';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateTourDto): Promise<TourDto> {
    return firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
  }

  async publish(id: string, dto: PriceDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, dto));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }

  async reactivate(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/reactivate`, {}));
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
  }

  async changeRecommendedGroupSize(id: string, dto: GroupSizeDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/recommended-group-size`, dto));
  }

  async addKeyPoint(id: string, dto: AddKeyPointDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/key-points`, dto));
  }

  async changeKeyPoint(id: string, keyPointId: string, dto: ChangeKeyPointDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/key-points/${keyPointId}`, dto));
  }

  async removeKeyPoint(id: string, keyPointId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/key-points/${keyPointId}`));
  }

  async recordLength(id: string, dto: TourLengthDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/length`, dto));
  }

  async uploadImage(file: File): Promise<UploadedImageDto> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<UploadedImageDto>(IMAGES_URL, body));
  }
}
