export interface UpdateHoldRequest {
  quantity: number;
  purchasePrice: number;
  purchaseDate: string;
  exchange: number;
  notes?: string;
}
