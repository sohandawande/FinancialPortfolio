export interface SystemLogDetail {
  exception?: string | null;
  stackTrace?: string | null;
}

export interface SystemLog {
  id: number;
  logLevel: string;
  applicationLevel: string;
  category: string;
  method?: string | null;
  message: string;
  userId?: number | null;
  identityUserId?: string | null;
  requestPath?: string | null;
  ipAddress?: string | null;
  machineName?: string | null;
  detail?: SystemLogDetail | null;
  createdBy?: number | null;
  modifiedBy?: number | null;
  createdDate: string;
  modifiedDate: string;
}