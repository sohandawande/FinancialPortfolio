import { Component, inject, ChangeDetectionStrategy, computed } from '@angular/core';

import { ConfirmModalService } from '../../../core/services/confirm-modal/confirm-modal.service';
import { FpModal } from '../fp-modal/fp-modal';
import { FpModalVariant } from '../../../core/models/fp-modal/fp-modal.model';

@Component({
  selector: 'app-confirm-modal',
  standalone: true,
  imports: [FpModal],
  templateUrl: './confirm-modal.html',
  styleUrl: './confirm-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmModal {
  readonly modal = inject(ConfirmModalService);

  readonly variant = computed<FpModalVariant>(() => {
    const color = this.modal.options().confirmColor;
    if (color === 'danger' || color === 'success' || color === 'warning') return color;
    return 'default';
  });

  readonly icon = computed(() => {
    switch (this.modal.options().confirmColor) {
      case 'danger':
        return 'bi-exclamation-triangle';
      case 'success':
        return 'bi-check-circle';
      case 'warning':
        return 'bi-exclamation-circle';
      default:
        return 'bi-question-circle';
    }
  });
}
