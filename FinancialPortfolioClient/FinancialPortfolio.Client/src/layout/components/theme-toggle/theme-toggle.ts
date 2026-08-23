import { Component, inject, ChangeDetectionStrategy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../../core/services/theme/theme.service';
import { ThemeMode } from '../../../core/models/theme/theme.model';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './theme-toggle.html',
  styleUrl: './theme-toggle.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ThemeToggle {
  readonly theme = inject(ThemeService);

  readonly icon = computed(() => {
    switch (this.theme.mode()) {
      case 'light':
        return 'bi-sun-fill';
      case 'dark':
        return 'bi-moon-stars-fill';
      default:
        return 'bi-circle-half';
    }
  });

  readonly label = computed(() => {
    switch (this.theme.mode()) {
      case 'light':
        return 'Light';
      case 'dark':
        return 'Dark';
      default:
        return 'Auto';
    }
  });

  setMode(mode: ThemeMode): void {
    this.theme.setMode(mode);
  }
}