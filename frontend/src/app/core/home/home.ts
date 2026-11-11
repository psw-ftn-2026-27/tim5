import { Component, inject } from '@angular/core';
import { Auth } from '../auth/auth';
import { Discover } from './discover/discover';
import { Hero } from './hero/hero';
import { Gallery } from './gallery/gallery';
import { WhyExplorer } from './why-explorer/why-explorer';
import { Footer } from './footer/footer';

@Component({
  selector: 'app-home',
  imports: [Hero, Discover, Gallery, WhyExplorer, Footer],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly auth = inject(Auth);
}
