import { Component, HostListener, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { LayoutService } from '../core/services/layout/layout.service';
import { Toast } from '../layout/components/toast/toast';
import { ConfirmModal } from '../layout/components/confirm-modal/confirm-modal';
import { LoadingOverlay } from '../layout/components/loading-overlay/loading-overlay';
import { ThemeService } from '../core/services/theme/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, Toast, ConfirmModal, LoadingOverlay],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly layoutService = inject(LayoutService);
  private readonly theme = inject(ThemeService);

  constructor() {
    this.layoutService.updateScreen(window.innerWidth);
  }

  @HostListener('window:resize')
  onResize() {
    this.layoutService.updateScreen(window.innerWidth);
  }
}
