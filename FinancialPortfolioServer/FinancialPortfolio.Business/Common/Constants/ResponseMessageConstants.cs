namespace FinancialPortfolio.Business.Common.Constants
{
    public static class ResponseMessageConstants
    {
        public const string RegistrationSuccess = "User registered successfully.";

        public const string RegistrationFailed = "Registration failed.";

        public const string LoginSuccess = "Login successful.";

        public const string LoginFailed = "Invalid email or password.";

        public const string LogoutSuccess = "Logout successful.";
        public const string LogoutFailed = "Logout failed.";

        public const string RefreshTokenSuccess = "Token refreshed successfully.";
        public const string RefreshTokenExpired = "Refresh token has expired.";
        public const string RefreshTokenInactive = "Refresh token is inactive.";
        public const string RefreshTokenRevoked = "Refresh token has been revoked.";
        public const string RefreshTokenNotFound = "Refresh token not found.";
        public const string RefreshTokenInvalid = "Invalid access token.";

        public const string Unauthorized = "Unauthorized.";

        public const string Forbidden = "Forbidden.";

        public const string ValidationFailed = "Validation failed.";

        public const string ForgotPasswordSuccess = "If the email exists, a password reset link has been sent.";

        public const string ResetPasswordSuccess = "Password has been reset successfully.";
        public const string ResetPasswordFailed = "Invalid or expired reset token.";

        public const string ChangePasswordSuccess = "Password changed successfully.";
        public const string ChangePasswordFailed = "Current password is incorrect.";

        public const string UserNotFound = "User not found.";

        public const string UserAlreadyExists = "User already exists.";

        public const string EmailAlreadyExists = "Email already exists.";

        public const string UserNameAlreadyExists = "Username already exists.";
        public const string RegisterSuccess = "Register success.";

        public const string InvalidToken = "Invalid Token";
        public const string InvalidRefreshToken = "Invalid Refresh Token";
    }
}
