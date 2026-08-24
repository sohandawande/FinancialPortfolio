export interface PortfolioDividendTrackerRow {
  stockId: number;
  symbol: string;
  companyName: string;
  logoUrl?: string | null;
  currentQuantity: number;
  invested: number;
  thisYearAmount: number;
  lifetimeAmount: number;
  yieldOnCostPercent: number;
  payoutCount: number;
  lastDividendDate?: string | null;
}
