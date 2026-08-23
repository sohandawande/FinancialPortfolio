namespace FinancialPortfolio.Models.Model.Response.Authentication
{
    using System;

    /// <summary>
    /// Defines the <see cref="LoginResponse" />
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether IsSuccess
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the AccessToken
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RefreshToken
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Expiration
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}
