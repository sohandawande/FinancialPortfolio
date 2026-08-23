export interface Portfolio {
  id: number;
  userId: number;
  name: string;
  description?: string | null;
  isActive: boolean;
  createdBy?: string | null;
  modifiedBy?: string | null;
  createdDate: string;
  modifiedDate?: string | null;
}