export interface ManagedUser {
  identityUserId: string;
  userId: number;
  userCode: string;
  fullName: string;
  email: string;
  mobileNumber: string;
  isActive: boolean;
  roles: string[];
  createdDate?: string;
  modifiedDate?: string;
}