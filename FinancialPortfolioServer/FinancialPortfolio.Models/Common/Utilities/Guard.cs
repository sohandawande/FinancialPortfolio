namespace FinancialPortfolio.Models.Common.Utilities
{
    public static class Guard
    {
        /// <summary>
        /// Throws an exception if the value is null.
        /// </summary>
        public static T AgainstNull<T>(
            T? value,
            string parameterName)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(value, parameterName);

            return value;
        }

        /// <summary>
        /// Throws an exception if the string is null or whitespace.
        /// </summary>
        public static string AgainstNullOrWhiteSpace(
            string? value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"{parameterName} cannot be null or empty.",
                    parameterName);
            }

            return value;
        }

        /// <summary>
        /// Throws an exception if the Guid is empty.
        /// </summary>
        public static Guid AgainstEmpty(
            Guid value,
            string parameterName)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException(
                    $"{parameterName} cannot be empty.",
                    parameterName);
            }

            return value;
        }

        /// <summary>
        /// Throws an exception if the long value is less than or equal to zero.
        /// </summary>
        public static long AgainstNonPositive(
            long value,
            string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    $"{parameterName} must be greater than zero.");
            }

            return value;
        }

        /// <summary>
        /// Throws an exception if the collection is null or empty.
        /// </summary>
        public static IReadOnlyCollection<T> AgainstNullOrEmpty<T>(
            IReadOnlyCollection<T>? collection,
            string parameterName)
        {
            if (collection is null || collection.Count == 0)
            {
                throw new ArgumentException(
                    $"{parameterName} cannot be null or empty.",
                    parameterName);
            }

            return collection;
        }
    }
}
