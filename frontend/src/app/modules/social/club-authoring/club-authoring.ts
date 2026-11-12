import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateClubDto, UploadedImageDto } from '../api/social-api-types';

const BASE_URL = '/api/social/clubs';
const IMAGES_URL = '/api/social/images';

@Injectable({ providedIn: 'root' })
export class ClubAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateClubDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async uploadImage(file: File): Promise<UploadedImageDto> {
    const body = new FormData();
    body.append('file', file);
    return firstValueFrom(this.http.post<UploadedImageDto>(IMAGES_URL, body));
  }

  async deleteImage(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${IMAGES_URL}/${id}`));
  }
}
