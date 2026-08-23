import {
  Component,
  input,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { getPasswordStrength } from '../../../core/helper/validators/password-strength.helper';

@Component({
  selector: 'app-password-strength',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './password-strength.html',
  styleUrl: './password-strength.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PasswordStrength {
  readonly password = input<string>('');

  readonly result = computed(() => getPasswordStrength(this.password()));

  readonly barClass = computed(() => {
    const score = this.result().score;
    if (score <= 1) return 'bg-danger';
    if (score === 2) return 'bg-warning';
    if (score === 3) return 'bg-info';
    return 'bg-success';
  });
}