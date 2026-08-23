export interface PortfolioDividend {
  id: number;
  portfolioId: number;
  stockId: number;
  symbol: string;
  companyName: string;
  logoUrl?: string | null;
  exchange: number;
  quantity: number;
  perShareAmount: number;
  amount: number;
  dividendDate: string;
  exDate?: string | null;
  recordDate?: string | null;
  notes?: string | null;
  createdDate?: string;
  modifiedDate?: string;
}
