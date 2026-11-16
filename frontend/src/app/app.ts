import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AnalyticsConsent } from './core/analytics/analytics-consent';
import { ConsentBanner } from './core/analytics/consent-banner/consent-banner';
import { Auth } from './core/auth/auth';

@Component({
  imports: [ConsentBanner, RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly consent = inject(AnalyticsConsent);
}
