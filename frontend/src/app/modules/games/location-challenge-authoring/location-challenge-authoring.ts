import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  HiddenPointInputDto,
  LocationChallengeDto,
  LocationChallengeInputDto,
  UploadedImageDto,
} from '../api/games-api-types';

const BASE_URL = '/api/games/location-challenges';
const IMAGES_URL = '/api/games/images';

@Injectable({ providedIn: 'root' })
export class LocationChallengeAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: LocationChallengeInputDto): Promise<LocationChallengeDto> {
    return firstValueFrom(this.http.post<LocationChallengeDto>(BASE_URL, dto));
  }

  async update(id: string, dto: LocationChallengeInputDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }

  async addHiddenPoint(id: string, dto: HiddenPointInputDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/hidden-points`, dto));
  }

  async changeHiddenPoint(id: string, pointId: string, dto: HiddenPointInputDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}/hidden-points/${pointId}`, dto));
  }

  async removeHiddenPoint(id: string, pointId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}/hidden-points/${pointId}`));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, null));
  }

  async retire(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/retire`, null));
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, null));
  }

  async delete(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/${id}`));
  }

  async uploadImage(file: File): Promise<UploadedImageDto> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<UploadedImageDto>(IMAGES_URL, body));
  }
}
