import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { PortfolioDividendYearTotal } from '../../../core/models/portfolio/portfolio-dividend-year-total.model';

@Component({
  selector: 'app-dividend-year-totals',
  standalone: true,
  imports: [CurrencyPipe],
  templateUrl: './dividend-year-totals.html',
  styleUrl: './dividend-year-totals.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DividendYearTotals {
  readonly title = input('Dividends by year');
  readonly lifetime = input(0);
  readonly rows = input<PortfolioDividendYearTotal[]>([]);
  readonly emptyText = input('No dividends recorded yet.');

  readonly hasRows = computed(() => this.rows().length > 0);
}