import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeSummaryDto } from '../../api/games-api-types';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';

@Component({
  imports: [RouterLink],
  selector: 'app-location-challenge-list',
  styleUrl: './location-challenge-list.scss',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  private readonly locationChallengeAuthoring = inject(LocationChallengeAuthoring);

  protected readonly challenges = httpResource<LocationChallengeSummaryDto[]>(
    () => '/api/games/location-challenges',
  );

  protected readonly actionPending = signal(false);
  protected readonly actionError = signal<string | null>(null);

  protected async publish(challengeId: string): Promise<void> {
    this.actionPending.set(true);
    this.actionError.set(null);
    try {
      await this.locationChallengeAuthoring.publish(challengeId);
      this.challenges.reload();
    } catch (failure) {
      this.actionError.set(serverMessage(failure, 'Could not publish the location challenge.'));
      this.challenges.reload();
    } finally {
      this.actionPending.set(false);
    }
  }

  protected async retire(challengeId: string): Promise<void> {
    this.actionPending.set(true);
    this.actionError.set(null);
    try {
      await this.locationChallengeAuthoring.retire(challengeId);
      this.challenges.reload();
    } catch (failure) {
      this.actionError.set(serverMessage(failure, 'Could not retire the location challenge.'));
      this.challenges.reload();
    } finally {
      this.actionPending.set(false);
    }
  }

  protected async archive(challengeId: string): Promise<void> {
    this.actionPending.set(true);
    this.actionError.set(null);
    try {
      await this.locationChallengeAuthoring.archive(challengeId);
      this.challenges.reload();
    } catch (failure) {
      this.actionError.set(serverMessage(failure, 'Could not archive the location challenge.'));
      this.challenges.reload();
    } finally {
      this.actionPending.set(false);
    }
  }

  protected async delete(challengeId: string): Promise<void> {
    this.actionPending.set(true);
    this.actionError.set(null);
    try {
      await this.locationChallengeAuthoring.delete(challengeId);
      this.challenges.reload();
    } catch (failure) {
      this.actionError.set(serverMessage(failure, 'Could not delete the location challenge.'));
      this.challenges.reload();
    } finally {
      this.actionPending.set(false);
    }
  }
}
