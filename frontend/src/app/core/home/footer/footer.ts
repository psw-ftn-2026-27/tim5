import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home-footer',
  imports: [RouterLink],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
})
export class Footer {
  readonly loggedIn = input.required<boolean>();
  readonly showCookieSettings = input(false);

  readonly cookieSettings = output();
}
