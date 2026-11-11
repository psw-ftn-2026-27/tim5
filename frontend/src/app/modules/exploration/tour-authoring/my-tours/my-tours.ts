import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  // At most one form is open at a time: which tour it belongs to and which form it is.
  protected readonly activeTourId = signal<string | null>(null);
  protected readonly activeForm = signal<'transport-time' | 'group-size' | 'publish' | null>(
    null,
  );
  protected readonly pending = signal(false);

  // An error always belongs to one tour and is shown under that tour's row.
  protected readonly error = signal<string | null>(null);
  protected readonly errorTourId = signal<string | null>(null);

  // While a form is open or a command is running, the table actions are disabled.
  protected readonly actionsLocked = computed(() => this.activeForm() !== null || this.pending());

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  private readonly groupSizeModel = signal({
    min: null as number | null,
    max: null as number | null,
  });

  protected readonly groupSizeForm = form(this.groupSizeModel, (path) => {
    required(path.min, { message: 'Min is required.' });
    min(path.min, 1, { message: 'Min must be at least 1.' });
    required(path.max, { message: 'Max is required.' });
  });

  protected readonly priceChoice = signal<'Free' | 'Paid' | null>(null);

  private readonly priceModel = signal({ amount: null as number | null });

  protected readonly priceForm = form(this.priceModel, (path) => {
    required(path.amount, { message: 'Amount is required.' });
    min(path.amount, 1, { message: 'Amount must be greater than 0.' });
  });

  protected closeForm(): void {
    this.activeTourId.set(null);
    this.activeForm.set(null);
    this.clearError();
  }

  protected selectTransportTime(tourId: string): void {
    this.openForm(tourId, 'transport-time');
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

  protected selectGroupSize(tour: TourDto): void {
    this.openForm(tour.id, 'group-size');
    this.groupSizeForm().reset({
      min: tour.recommendedGroupSize?.min ?? null,
      max: tour.recommendedGroupSize?.max ?? null,
    });
  }

  protected selectPublish(tourId: string): void {
    this.openForm(tourId, 'publish');
    this.cancelPrice();
  }

  protected choosePrice(choice: 'Free' | 'Paid'): void {
    this.priceChoice.set(choice);
  }

  protected cancelPrice(): void {
    this.priceChoice.set(null);
    this.priceForm().reset({ amount: null });
  }

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.activeTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.clearError();
    try {
      await this.tourAuthoring.addTransportTime(tourId, {
        transport: this.form.transport().value(),
        minutes: this.form.minutes().value(),
      });
      this.tours.reload();
      this.closeForm();
    } catch (failure) {
      this.showError(tourId, serverMessage(failure, 'Could not add the transport time.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async changeRecommendedGroupSize(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.activeTourId();
    const groupSizeMin = this.groupSizeForm.min().value();
    const groupSizeMax = this.groupSizeForm.max().value();
    if (tourId === null || groupSizeMin === null || groupSizeMax === null) {
      return;
    }
    this.pending.set(true);
    this.clearError();
    try {
      await this.tourAuthoring.changeRecommendedGroupSize(tourId, {
        min: groupSizeMin,
        max: groupSizeMax,
      });
      this.tours.reload();
      this.closeForm();
    } catch (failure) {
      this.showError(
        tourId,
        serverMessage(failure, 'Could not change the recommended group size.'),
      );
    } finally {
      this.pending.set(false);
    }
  }

  protected async publish(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.activeTourId();
    const choice = this.priceChoice();
    const amount = choice === 'Free' ? 0 : this.priceForm.amount().value();
    if (tourId === null || choice === null || amount === null) {
      return;
    }
    this.pending.set(true);
    this.clearError();
    try {
      await this.tourAuthoring.publish(tourId, { isFree: choice === 'Free', amount: amount });
      this.tours.reload();
      this.closeForm();
    } catch (failure) {
      this.showError(tourId, serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(tourId: string): Promise<void> {
    this.pending.set(true);
    this.clearError();
    try {
      await this.tourAuthoring.archive(tourId);
      this.tours.reload();
    } catch (failure) {
      this.showError(tourId, serverMessage(failure, 'Could not archive the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reactivate(tourId: string): Promise<void> {
    this.pending.set(true);
    this.clearError();
    try {
      await this.tourAuthoring.reactivate(tourId);
      this.tours.reload();
    } catch (failure) {
      this.showError(tourId, serverMessage(failure, 'Could not reactivate the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  private openForm(tourId: string, kind: 'transport-time' | 'group-size' | 'publish'): void {
    this.activeTourId.set(tourId);
    this.activeForm.set(kind);
    this.clearError();
  }

  private showError(tourId: string, message: string): void {
    this.errorTourId.set(tourId);
    this.error.set(message);
  }

  private clearError(): void {
    this.errorTourId.set(null);
    this.error.set(null);
  }
}
