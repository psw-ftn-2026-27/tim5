import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home-discover',
  imports: [RouterLink],
  templateUrl: './discover.html',
  styleUrl: './discover.scss',
})
export class Discover {
  readonly loggedIn = input.required<boolean>();
}
