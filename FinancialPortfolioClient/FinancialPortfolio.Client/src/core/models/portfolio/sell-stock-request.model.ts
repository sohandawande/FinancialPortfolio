export interface SellStockRequest {
  stockId: number;
  sellQuantity: number;
  sellPrice: number;
  soldDate: string;
  notes?: string;
}