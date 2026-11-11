import { Component, OnDestroy, computed, signal } from '@angular/core';

@Component({
  selector: 'app-home-gallery',
  templateUrl: './gallery.html',
  styleUrl: './gallery.scss',
})
export class Gallery implements OnDestroy {
  protected readonly slides = [
    {
      category: 'Cities & Culture',
      title: 'Every street has a story.',
      description: 'Take the long way through old streets. Let the details become the memories.',
      location: 'Venice, Italy',
      image: '/images/home/gallery-city.webp',
    },
    {
      category: 'Into the Wild',
      title: 'Leave room for wonder.',
      description: 'Follow the trail beyond the familiar, where quiet lakes meet unforgettable peaks.',
      location: 'Sorapiss, Italy',
      image: '/images/home/gallery-nature.webp',
    },
    {
      category: 'Coastal Escapes',
      title: 'Find a slower kind of day.',
      description: 'Follow the curve of the coast. Make time for clear water, quiet coves and the open sea.',
      location: 'Cala dei Gabbiani, Sardinia, Italy',
      image: '/images/home/gallery-coast.webp',
    },
  ];
  protected readonly activeSlide = signal(0);
  protected readonly currentSlide = computed(() => this.slides[this.activeSlide()]);
  protected readonly playing = signal(false);
  private readonly motionPreference = window.matchMedia('(prefers-reduced-motion: reduce)');
  protected readonly reducedMotion = signal(this.motionPreference.matches);
  private timer: number | undefined;

  private readonly onMotionChange = (event: MediaQueryListEvent): void => {
    this.reducedMotion.set(event.matches);
    // Restoring the preference never restarts playback without an explicit Play.
    if (event.matches) {
      this.pause();
    }
  };

  constructor() {
    this.motionPreference.addEventListener('change', this.onMotionChange);
    this.play();
  }

  protected play(): void {
    if (this.reducedMotion()) {
      return;
    }
    this.playing.set(true);
    this.restartTimer();
  }

  protected stopTimer(): void {
    if (this.timer !== undefined) {
      window.clearInterval(this.timer);
      this.timer = undefined;
    }
  }

  protected restartTimer(): void {
    this.stopTimer();
    if (!this.playing() || this.reducedMotion()) {
      return;
    }
    this.timer = window.setInterval(() => {
      this.activeSlide.update((index) => (index + 1) % this.slides.length);
    }, 6000);
  }

  protected pause(): void {
    this.stopTimer();
    this.playing.set(false);
  }

  protected togglePlayback(): void {
    if (this.playing()) {
      this.pause();
    } else {
      this.play();
    }
  }

  protected select(index: number): void {
    this.activeSlide.set(index);
    this.restartTimer();
  }

  protected previous(): void {
    this.select((this.activeSlide() + this.slides.length - 1) % this.slides.length);
  }

  protected next(): void {
    this.select((this.activeSlide() + 1) % this.slides.length);
  }

  ngOnDestroy(): void {
    this.pause();
    this.motionPreference.removeEventListener('change', this.onMotionChange);
  }
}
