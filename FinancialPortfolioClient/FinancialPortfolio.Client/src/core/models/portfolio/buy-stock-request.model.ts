export interface BuyStockRequest {
  stockId: number;
  quantity: number;
  purchasePrice: number;
  purchaseDate: string;       // ISO date
  exchange: number;           // 1=NSE, 2=BSE
  notes?: string;
}