export interface MarketDataSyncRequest {
  forceRefresh: boolean;
  tradeDate?: string | null;
}

export interface MarketDataSyncResponse {
  source: string;
  tradeDate?: string | null;
  totalRecords: number;
  insertedRecords: number;
  updatedRecords: number;
  skippedRecords: number;
  nseRecords: number;
  fundamentalRecords?: number;
  mcapUpdated: number;
  capClassified: number;
  peUpdated?: number;
  epsUpdated?: number;
  week52Updated?: number;
  industryUpdated?: number;
}
