namespace FinancialPortfolio.Models.Model.Request.Authentication
{
    public class LoginRequest
    {
        /// <summary>Email or username or usercode</summary>
        public string LoginId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
