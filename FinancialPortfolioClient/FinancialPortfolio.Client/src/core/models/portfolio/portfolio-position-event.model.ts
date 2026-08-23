export interface PortfolioPositionEvent {
  eventType: string;
  eventDate: string;
  quantity?: number | null;
  price?: number | null;
  amount: number;
  notes?: string | null;
  sourceId?: number | null;
}
