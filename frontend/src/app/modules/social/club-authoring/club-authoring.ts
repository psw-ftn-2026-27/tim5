import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreateClubDto } from '../api/social-api-types';

const BASE_URL = '/api/social/clubs';

@Injectable({ providedIn: 'root' })
export class ClubAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: CreateClubDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }
}
