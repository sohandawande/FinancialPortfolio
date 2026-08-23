export interface UpdateDividendRequest {
  stockId: number;
  quantity: number;
  perShareAmount: number;
  amount?: number;
  dividendDate: string;
  exDate?: string;
  recordDate?: string;
  notes?: string;
}
