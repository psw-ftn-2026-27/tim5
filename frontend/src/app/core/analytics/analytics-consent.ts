import { Injectable, computed, signal } from '@angular/core';
import { environment } from '../../../environments/environment';

const CONSENT_KEY = 'explorer.analytics-consent';
const MEASUREMENT_ID = environment.gaMeasurementId;

type Consent = 'granted' | 'denied';

// The globals gtag.js reads and writes. The index signature covers the
// 'ga-disable-<ID>' flag, whose name is built from the measurement ID.
interface AnalyticsWindow {
  dataLayer?: unknown[];
  gtag?: (...args: unknown[]) => void;
  [key: string]: unknown;
}

function readConsent(): Consent | null {
  const stored = localStorage.getItem(CONSENT_KEY);
  return stored === 'granted' || stored === 'denied' ? stored : null;
}

function deleteAnalyticsCookies(): void {
  const host = location.hostname;
  const parent = host.split('.').slice(1).join('.');
  const expired = '=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
  for (const cookie of document.cookie.split('; ')) {
    const name = cookie.split('=')[0];
    if (name === '_ga' || name.startsWith('_ga_')) {
      document.cookie = name + expired;
      document.cookie = name + expired + '; domain=' + host;
      if (parent !== '') {
        document.cookie = name + expired + '; domain=' + parent;
      }
    }
  }
}

@Injectable({ providedIn: 'root' })
export class AnalyticsConsent {
  private readonly window = window as unknown as AnalyticsWindow;
  private readonly consent = signal<Consent | null>(readConsent());
  private readonly reopened = signal(false);
  private scriptAdded = false;

  readonly isConfigured = MEASUREMENT_ID !== '';
  readonly bannerVisible = computed(
    () => this.isConfigured && (this.consent() === null || this.reopened()),
  );

  constructor() {
    if (this.isConfigured && this.consent() === 'granted') {
      this.startTracking();
    }
  }

  accept(): void {
    this.store('granted');
    this.startTracking();
  }

  decline(): void {
    this.store('denied');
    this.window['ga-disable-' + MEASUREMENT_ID] = true;
    deleteAnalyticsCookies();
  }

  reopen(): void {
    this.reopened.set(true);
  }

  private store(consent: Consent): void {
    localStorage.setItem(CONSENT_KEY, consent);
    this.consent.set(consent);
    this.reopened.set(false);
  }

  // The 'config' call records the current page view; later route changes are
  // recorded by GA4 enhanced measurement, so the router is not observed here.
  private startTracking(): void {
    this.window['ga-disable-' + MEASUREMENT_ID] = false;
    if (!this.scriptAdded) {
      this.addScript();
    }
    this.gtag('config', MEASUREMENT_ID);
  }

  // If the script is blocked (an ad blocker), the calls below only fill
  // dataLayer and nothing else happens, so the app keeps working.
  private addScript(): void {
    this.scriptAdded = true;
    const dataLayer: unknown[] = this.window.dataLayer ?? [];
    this.window.dataLayer = dataLayer;
    this.window.gtag = function () {
      // gtag.js expects the arguments object itself, not an array.
      // eslint-disable-next-line prefer-rest-params
      dataLayer.push(arguments);
    };
    this.gtag('js', new Date());

    const script = document.createElement('script');
    script.async = true;
    script.src = 'https://www.googletagmanager.com/gtag/js?id=' + MEASUREMENT_ID;
    document.head.appendChild(script);
  }

  private gtag(...args: unknown[]): void {
    this.window.gtag?.(...args);
  }
}
