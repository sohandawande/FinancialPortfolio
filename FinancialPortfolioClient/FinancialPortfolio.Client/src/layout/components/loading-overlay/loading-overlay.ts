import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingOverlayService } from '../../../core/services/loading-overlay/loading-overlay.service';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading-overlay.html',
  styleUrl: './loading-overlay.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingOverlay {
  readonly loading = inject(LoadingOverlayService);
}