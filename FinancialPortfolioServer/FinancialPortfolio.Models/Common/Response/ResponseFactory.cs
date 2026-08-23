namespace FinancialPortfolio.Models.Common.Response
{
    /// <summary>
    /// Defines the <see cref="ResponseFactory" />
    /// </summary>
    public static class ResponseFactory
    {
        /// <summary>
        /// The Success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data<see cref="T"/></param>
        /// <param name="message">The message<see cref="string"/></param>
        /// <returns>The <see cref="ApiResponse{T}"/></returns>
        public static ApiResponse<T> Success<T>(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// The Failure
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message<see cref="string"/></param>
        /// <param name="errors">The errors<see cref="string[]"/></param>
        /// <returns>The <see cref="ApiResponse{T}"/></returns>
        public static ApiResponse<T> Failure<T>(
            string message,
            params string[] errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors.ToList()
            };
        }
    }
}
