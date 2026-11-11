import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required, validate } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeAuthoring } from '../social-challenge-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-social-challenge',
  styleUrl: './create-social-challenge.scss',
  templateUrl: './create-social-challenge.html',
})
export class CreateSocialChallenge {
  private readonly socialChallengeAuthoring = inject(SocialChallengeAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      start: '',
      end: '',
      requiredPeople: null as number | null,
      rangeInMeters: null as number | null,
      xp: null as number | null,
      exitMessage: '',
    }),
    (path) => {
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
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const requiredPeople = this.form.requiredPeople().value();
    const rangeInMeters = this.form.rangeInMeters().value();
    const xp = this.form.xp().value();
    if (requiredPeople === null || rangeInMeters === null || xp === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.socialChallengeAuthoring.create({
        name: this.form.name().value(),
        exitMessage: this.form.exitMessage().value(),
        requiredPeople,
        rangeInMeters,
        xp,
        window: { start: this.form.start().value(), end: this.form.end().value() },
      });
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not create the challenge.'));
    } finally {
      this.pending.set(false);
    }
  }
}
