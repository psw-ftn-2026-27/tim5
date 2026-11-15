import { Component, output } from '@angular/core';

@Component({
  selector: 'app-consent-banner',
  templateUrl: './consent-banner.html',
  styleUrl: './consent-banner.scss',
  host: { role: 'region', 'aria-label': 'Cookie consent' },
})
export class ConsentBanner {
  readonly accepted = output();
  readonly declined = output();
}
