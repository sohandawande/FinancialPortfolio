namespace FinancialPortfolio.Models.Common.Response
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="ApiResponse{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether Success
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Gets or sets the Errors
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets the Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
