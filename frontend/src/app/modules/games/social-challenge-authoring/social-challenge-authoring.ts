import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SocialChallengeDto, SocialChallengeInputDto } from '../api/games-api-types';

const BASE_URL = '/api/games/challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengeAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: SocialChallengeInputDto): Promise<SocialChallengeDto> {
    return firstValueFrom(this.http.post<SocialChallengeDto>(BASE_URL, dto));
  }

  async update(id: string, dto: SocialChallengeInputDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
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
}
