export interface CurrentUser {
  identityUserId: string;
  userId: number;
  fullName: string;
  userCode: string;
  email: string;
  userName: string;
  roles: string[];
}