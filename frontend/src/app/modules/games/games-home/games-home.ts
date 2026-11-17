import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  imports: [RouterLink],
  selector: 'app-games-home',
  template: `
    <h1>Games</h1>
    <p><a routerLink="/games/quizzes">Quizzes</a></p>
    <p><a routerLink="/games/location-challenges">Location challenges</a></p>
    <p><a routerLink="/games/overview">All challenges</a></p>
  `,
})
export class GamesHome {}
