export interface PagedResponse<T> {
  data: T[];
  totalRecords: number;
  totalPages: number;
  pageNumber: number;
  pageSize: number;
}