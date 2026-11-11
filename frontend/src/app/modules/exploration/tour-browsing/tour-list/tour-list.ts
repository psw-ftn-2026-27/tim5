import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import {
  TourDifficulty,
  TourDto,
  TourSearchCriteriaDto,
  TourSortOrder,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

const pageSize = 20;

@Component({
  imports: [FormField, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  protected readonly form = form(
    signal({
      name: '',
      easy: false,
      moderate: false,
      hard: false,
      walking: false,
      bicycle: false,
      car: false,
      tags: '',
      sort: 'Newest' as TourSortOrder,
    }),
  );

  // The criteria of the last executed search. Paging reuses them, not the current form contents.
  private readonly criteria = signal<TourSearchCriteriaDto>({
    name: null,
    difficulties: null,
    tags: null,
    transports: null,
    sort: 'Newest',
  });

  protected readonly page = signal(1);

  protected readonly tours = httpResource<PageResult<TourDto>>(() => {
    const criteria = this.criteria();
    const parts = [`page=${this.page()}`, `pageSize=${pageSize}`, `sort=${criteria.sort}`];
    if (criteria.name !== null) {
      parts.push(`name=${encodeURIComponent(criteria.name)}`);
    }
    for (const difficulty of criteria.difficulties ?? []) {
      parts.push(`difficulties=${difficulty}`);
    }
    for (const tag of criteria.tags ?? []) {
      parts.push(`tags=${encodeURIComponent(tag)}`);
    }
    for (const transport of criteria.transports ?? []) {
      parts.push(`transports=${transport}`);
    }
    return `/api/exploration/tours/published?${parts.join('&')}`;
  });

  protected search(event: Event): void {
    event.preventDefault();
    const model = this.form().value();

    const difficulties: TourDifficulty[] = [];
    if (model.easy) {
      difficulties.push('Easy');
    }
    if (model.moderate) {
      difficulties.push('Moderate');
    }
    if (model.hard) {
      difficulties.push('Hard');
    }

    const transports: TransportMode[] = [];
    if (model.walking) {
      transports.push('Walking');
    }
    if (model.bicycle) {
      transports.push('Bicycle');
    }
    if (model.car) {
      transports.push('Car');
    }

    const tags = model.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');

    this.criteria.set({
      name: model.name === '' ? null : model.name,
      difficulties: difficulties.length > 0 ? difficulties : null,
      tags: tags.length > 0 ? tags : null,
      transports: transports.length > 0 ? transports : null,
      sort: model.sort,
    });
    this.page.set(1);
  }

  protected isLastPage(totalCount: number): boolean {
    return this.page() * pageSize >= totalCount;
  }

  protected previousPage(): void {
    this.page.update((page) => page - 1);
  }

  protected nextPage(): void {
    this.page.update((page) => page + 1);
  }
}
