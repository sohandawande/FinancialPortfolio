export interface PendingUser {
  identityUserId: string;
  userId: number;
  userCode: string;
  fullName: string;
  email: string;
  createdDate?: string;
  modifiedDate?: string;
}