import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ChallengeOverviewItemDto, ChallengeStatus } from '../../api/games-api-types';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-challenge-overview-list',
  styleUrl: './challenge-overview-list.scss',
  templateUrl: './challenge-overview-list.html',
})
export class ChallengeOverviewList {
  protected readonly filter = form(signal({ status: '' as ChallengeStatus | '' }));

  protected readonly challenges = httpResource<ChallengeOverviewItemDto[]>(() => {
    const status = this.filter.status().value();
    return status === '' ? '/api/games/overview' : `/api/games/overview?status=${status}`;
  });
}
