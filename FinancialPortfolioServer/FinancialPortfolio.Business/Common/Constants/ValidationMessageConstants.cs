namespace FinancialPortfolio.Business.Common.Constants
{
    public static class ValidationMessageConstants
    {
        public const string EmailAlreadyExists = "Email already exists.";
        public const string UserNameAlreadyExist = "Username already exists.";
        public const string DefaultRoleNotFound = "Default role not found.";
        public const string InvalidCredentials = "Invalid email, username, user code, or password.";
        public const string UserProfileNotFound = "User profile not found.";
        public const string UserAccountIsInactive = "This account is inactive. Ask an admin to activate it.";
        public const string UserNotFound = "User not found.";

        public const string LoginIdRequired = "Enter your email, username, or user code.";
        public const string PasswordRequired = "Password is required.";
        public const string PasswordMinLength = "Password must be at least 8 characters.";
        public const string PasswordComplexity = "Password must include a letter, a number, and a symbol.";
        public const string ConfirmPasswordRequired = "Confirm your password.";
        public const string PasswordsDoNotMatch = "Passwords do not match.";
        public const string CurrentPasswordRequired = "Current password is required.";
        public const string NewPasswordRequired = "New password is required.";
        public const string NewPasswordSameAsCurrent = "New password must be different from the current password.";
        public const string EmailRequired = "Email is required.";
        public const string EmailInvalid = "Enter a valid email address.";
        public const string UserNameRequired = "Username is required.";
        public const string UserNameFormat = "Username must be 3–50 characters: letters, numbers, dot, underscore, or hyphen.";
        public const string FirstNameRequired = "First name is required.";
        public const string LastNameRequired = "Last name is required.";
        public const string FirstNameFormat = "First name must be letters only (at least 2 characters).";
        public const string LastNameFormat = "Last name must be letters only (at least 2 characters).";
        public const string MobileRequired = "Mobile number is required.";
        public const string MobileInvalid = "Mobile number must be 10 digits.";
        public const string ResetTokenRequired = "Reset token is missing. Use the link from your email.";
        public const string AccessTokenRequired = "Access token is required.";
        public const string RefreshTokenRequired = "Refresh token is required.";

        public const string StockRequired = "Select a stock.";
        public const string QuantityMin = "Quantity must be at least 1.";
        public const string PurchasePriceMin = "Enter a purchase price greater than 0.";
        public const string PurchasePriceScale = "Purchase price can have at most 4 decimal places.";
        public const string PurchaseDateRequired = "Purchase date is required.";
        public const string PurchaseDateFuture = "Purchase date cannot be in the future.";
        public const string SellQuantityMin = "Sell quantity must be at least 1.";
        public const string SellPriceMin = "Enter a sell price greater than 0.";
        public const string SellPriceScale = "Sell price can have at most 4 decimal places.";
        public const string SoldDateRequired = "Sold date is required.";
        public const string SoldDateFuture = "Sold date cannot be in the future.";
        public const string ExchangeInvalid = "Choose NSE or BSE.";
        public const string NotesMax = "Notes cannot exceed 1000 characters.";
        public const string PortfolioNameRequired = "Portfolio name is required.";
        public const string PortfolioNameMax = "Portfolio name cannot exceed 100 characters.";
        public const string PortfolioDescriptionMax = "Description cannot exceed 500 characters.";
        public const string DividendAmountRequired = "Enter a per-share amount or a total dividend amount.";
        public const string DividendDateRequired = "Dividend date is required.";
        public const string DividendDateFuture = "Dividend date cannot be in the future.";
        public const string PerShareInvalid = "Per-share amount cannot be negative.";
        public const string AmountInvalid = "Dividend amount must be greater than 0.";

        public const string SymbolRequired = "Symbol is required.";
        public const string SymbolMax = "Symbol cannot exceed 50 characters.";
        public const string CompanyNameRequired = "Company name is required.";
        public const string CompanyNameMax = "Company name cannot exceed 250 characters.";
        public const string IndustryRequired = "Industry is required.";
        public const string IsinRequired = "ISIN is required.";
        public const string SeriesRequired = "Series is required.";
        public const string CurrentPriceMin = "Current price must be greater than 0.";
        public const string MarketCapMin = "Market cap must be greater than 0.";
    }
}
