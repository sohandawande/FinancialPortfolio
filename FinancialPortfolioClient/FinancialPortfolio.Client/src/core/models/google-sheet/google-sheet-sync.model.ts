export interface GoogleSheetSyncRequest {
  forceRefresh: boolean;
}

export interface GoogleSheetSyncResponse {
  totalRecords: number;
  insertedRecords: number;
  updatedRecords: number;
  skippedRecords: number;
}