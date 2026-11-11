import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ChallengeStatus, SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-social-challenge-list',
  styleUrl: './social-challenge-list.scss',
  templateUrl: './social-challenge-list.html',
})
export class SocialChallengeList {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);

  protected readonly filter = form(signal({ status: '' as ChallengeStatus | '' }));

  protected readonly challenges = httpResource<SocialChallengeDto[]>(() => {
    const status = this.filter.status().value();
    return status === '' ? '/api/games/challenges' : `/api/games/challenges?status=${status}`;
  });

  protected readonly editedChallengeId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly editModel = signal({
    name: '',
    start: '',
    end: '',
    requiredPeople: null as number | null,
    rangeInMeters: null as number | null,
    xp: null as number | null,
    exitMessage: '',
  });

  protected readonly editForm = form(this.editModel, (path) => {
    required(path.name, { message: 'Name is required.' });
    required(path.start, { message: 'Start is required.' });
    required(path.end, { message: 'End is required.' });
    validate(path.end, ({ value, valueOf }) =>
      value() !== '' && value() === valueOf(path.start)
        ? { kind: 'sameAsStart', message: 'End must differ from start.' }
        : null,
    );
    required(path.requiredPeople, { message: 'Required people is required.' });
    min(path.requiredPeople, 1, { message: 'Required people must be at least 1.' });
    required(path.rangeInMeters, { message: 'Range is required.' });
    min(path.rangeInMeters, 1, { message: 'Range must be greater than 0.' });
    required(path.xp, { message: 'XP is required.' });
    min(path.xp, 1, { message: 'XP must be greater than 0.' });
    required(path.exitMessage, { message: 'Exit message is required.' });
  });

  protected selectForEdit(challenge: SocialChallengeDto): void {
    this.error.set(null);
    this.editedChallengeId.set(challenge.id);
    this.editForm().reset({
      name: challenge.name,
      start: challenge.window.start.slice(0, 5),
      end: challenge.window.end.slice(0, 5),
      requiredPeople: challenge.requiredPeople,
      rangeInMeters: challenge.rangeInMeters,
      xp: challenge.xp,
      exitMessage: challenge.exitMessage,
    });
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editedChallengeId.set(null);
  }

  protected async update(event: Event): Promise<void> {
    event.preventDefault();
    const challengeId = this.editedChallengeId();
    const requiredPeople = this.editForm.requiredPeople().value();
    const rangeInMeters = this.editForm.rangeInMeters().value();
    const xp = this.editForm.xp().value();
    if (challengeId === null || requiredPeople === null || rangeInMeters === null || xp === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.update(challengeId, {
        name: this.editForm.name().value(),
        exitMessage: this.editForm.exitMessage().value(),
        requiredPeople,
        rangeInMeters,
        xp,
        window: { start: this.editForm.start().value(), end: this.editForm.end().value() },
      });
      this.editedChallengeId.set(null);
      this.filter().reset({ status: '' });
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not change the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
