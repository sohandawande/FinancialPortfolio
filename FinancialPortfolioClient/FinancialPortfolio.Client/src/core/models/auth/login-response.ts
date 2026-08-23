export interface LoginResponse {
  isSuccess: boolean;
  message: string;
  accessToken: string;
  refreshToken: string;
  expiration: string;
}