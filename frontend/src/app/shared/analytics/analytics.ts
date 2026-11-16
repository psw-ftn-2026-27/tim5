import { Injectable, inject } from '@angular/core';
import { AnalyticsConsent } from '../../core/analytics/analytics-consent';
import { Auth } from '../../core/auth/auth';

// The event catalog: one interface per event and one union member per interface,
// so only catalogued names and their parameters compile. See README.md next to
// this file before adding one.
interface TourPublished {
  name: 'tour_published';
  params: { tour_id: string };
}

export type AnalyticsEvent = TourPublished;

type Role = 'administrator' | 'explorer' | 'guest';

@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly consent = inject(AnalyticsConsent);
  private readonly auth = inject(Auth);

  // Adds the current user's role and nothing else about the user.
  record(event: AnalyticsEvent): void {
    this.consent.sendEvent(event.name, { ...event.params, role: this.role() });
  }

  private role(): Role {
    const user = this.auth.user();
    if (user === null) {
      return 'guest';
    }
    return user.roles.includes('administrator') ? 'administrator' : 'explorer';
  }
}
