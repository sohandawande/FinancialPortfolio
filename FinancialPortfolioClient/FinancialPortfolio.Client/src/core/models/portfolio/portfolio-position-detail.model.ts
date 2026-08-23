import { PortfolioDividend } from './portfolio-dividend.model';
import { PortfolioHolding } from './portfolio-holding.model';
import { PortfolioPosition } from './portfolio-position.model';
import { PortfolioPositionEvent } from './portfolio-position-event.model';
import { PortfolioSold } from './portfolio-sold.model';
import { PortfolioDividendYearTotal } from './portfolio-dividend-year-total.model';

export interface PortfolioPositionDetail {
  position: PortfolioPosition;
  buys: PortfolioHolding[];
  sells: PortfolioSold[];
  dividends: PortfolioDividend[];
  timeline: PortfolioPositionEvent[];
  dividendsByYear?: PortfolioDividendYearTotal[];
}
