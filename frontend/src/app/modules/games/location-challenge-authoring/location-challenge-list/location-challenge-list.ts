import { httpResource } from '@angular/common/http';
import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocationChallengeSummaryDto } from '../../api/games-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-location-challenge-list',
  styleUrl: './location-challenge-list.scss',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  protected readonly challenges = httpResource<LocationChallengeSummaryDto[]>(
    () => '/api/games/location-challenges',
  );
}
