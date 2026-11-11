import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
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

  protected readonly selectedTourId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  protected readonly groupSizeTourId = signal<string | null>(null);

  private readonly groupSizeModel = signal({
    min: null as number | null,
    max: null as number | null,
  });

  protected readonly groupSizeForm = form(this.groupSizeModel, (path) => {
    required(path.min, { message: 'Min is required.' });
    min(path.min, 1, { message: 'Min must be at least 1.' });
    required(path.max, { message: 'Max is required.' });
  });

  protected readonly publishTourId = signal<string | null>(null);
  protected readonly priceChoice = signal<'Free' | 'Paid' | null>(null);

  private readonly priceModel = signal({ amount: null as number | null });

  protected readonly priceForm = form(this.priceModel, (path) => {
    required(path.amount, { message: 'Amount is required.' });
    min(path.amount, 1, { message: 'Amount must be greater than 0.' });
  });

  protected selectGroupSize(tour: TourDto): void {
    this.error.set(null);
    this.groupSizeTourId.set(tour.id);
    this.groupSizeForm().reset({
      min: tour.recommendedGroupSize?.min ?? null,
      max: tour.recommendedGroupSize?.max ?? null,
    });
  }

  protected async changeRecommendedGroupSize(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.groupSizeTourId();
    const groupSizeMin = this.groupSizeForm.min().value();
    const groupSizeMax = this.groupSizeForm.max().value();
    if (tourId === null || groupSizeMin === null || groupSizeMax === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.changeRecommendedGroupSize(tourId, {
        min: groupSizeMin,
        max: groupSizeMax,
      });
      this.tours.reload();
      this.groupSizeTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not change the recommended group size.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected select(tourId: string): void {
    this.error.set(null);
    this.selectedTourId.set(tourId);
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

  protected selectPublish(tourId: string): void {
    this.error.set(null);
    this.publishTourId.set(tourId);
    this.cancelPrice();
  }

  protected choosePrice(choice: 'Free' | 'Paid'): void {
    this.priceChoice.set(choice);
  }

  protected closePublish(): void {
    this.publishTourId.set(null);
  }

  protected cancelPrice(): void {
    this.priceChoice.set(null);
    this.priceForm().reset({ amount: null });
  }

  protected async publish(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.publishTourId();
    const choice = this.priceChoice();
    const amount = choice === 'Free' ? 0 : this.priceForm.amount().value();
    if (tourId === null || choice === null || amount === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(tourId, { isFree: choice === 'Free', amount: amount });
      this.tours.reload();
      this.publishTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.archive(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not archive the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reactivate(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.reactivate(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reactivate the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.selectedTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.addTransportTime(tourId, {
        transport: this.form.transport().value(),
        minutes: this.form.minutes().value(),
      });
      this.tours.reload();
      this.selectedTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the transport time.'));
    } finally {
      this.pending.set(false);
    }
  }
}
