import { Component, input, output, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';

@Component({
  imports: [FormField],
  selector: 'app-deposit-form',
  styleUrl: './deposit-form.scss',
  templateUrl: './deposit-form.html',
})
export class DepositForm {
  readonly username = input.required<string>();
  readonly pending = input(false);

  readonly deposit = output<number>();

  protected readonly form = form(signal({ amount: null as number | null }), (path) => {
    required(path.amount, { message: 'Enter an amount.' });
    min(path.amount, 1, { message: 'The amount must be positive.' });
  });

  protected submit(event: Event): void {
    event.preventDefault();
    const amount = this.form.amount().value();
    if (amount !== null) {
      this.deposit.emit(amount);
    }
  }
}
