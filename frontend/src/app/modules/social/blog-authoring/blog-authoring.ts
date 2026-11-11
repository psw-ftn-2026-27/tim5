import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BlogDto, CreateBlogDto, UploadedImageDto } from '../api/social-api-types';

const BASE_URL = '/api/social/blogs';
const IMAGES_URL = '/api/social/images';

@Injectable({ providedIn: 'root' })
export class BlogAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateBlogDto): Promise<BlogDto> {
    return firstValueFrom(this.http.post<BlogDto>(BASE_URL, dto));
  }

  async publish(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, {}));
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
