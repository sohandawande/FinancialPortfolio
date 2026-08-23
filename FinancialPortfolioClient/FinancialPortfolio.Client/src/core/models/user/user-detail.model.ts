export interface UserDetail {
  identityUserId: string;
  userId: number;
  userName: string;
  userCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  mobileNumber: string;
  isActive: boolean;
  emailConfirmed: boolean;
  roles: string[];
  createdDate?: string;
  modifiedDate?: string;
}