import { Component, input, output, signal } from '@angular/core';

export interface PickedImage {
  key: string;
  previewUrl: string;
  uploading: boolean;
}

const MAX_SIZE_IN_BYTES = 5 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

@Component({
  imports: [],
  selector: 'app-image-picker',
  styleUrl: './image-picker.scss',
  templateUrl: './image-picker.html',
})
export class ImagePicker {
  readonly images = input.required<PickedImage[]>();
  readonly canAdd = input(true);
  readonly serverError = input<string | null>(null);

  readonly filePicked = output<File>();
  readonly imageRemoved = output<PickedImage>();

  protected readonly checkError = signal<string | null>(null);

  protected pick(event: Event): void {
    const fileInput = event.target as HTMLInputElement;
    const file = fileInput.files?.[0];
    // Clearing the value lets the user pick the same file again.
    fileInput.value = '';
    if (!file) {
      return;
    }

    if (!ALLOWED_TYPES.includes(file.type)) {
      this.checkError.set('Only JPEG, PNG and WebP images are allowed.');
      return;
    }
    if (file.size > MAX_SIZE_IN_BYTES) {
      this.checkError.set('The image must not be larger than 5 MB.');
      return;
    }

    this.checkError.set(null);
    this.filePicked.emit(file);
  }
}
