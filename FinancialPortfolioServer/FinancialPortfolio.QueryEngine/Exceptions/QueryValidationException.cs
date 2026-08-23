using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Exceptions
{
    public class QueryValidationException : Exception
    {
        public List<string> Errors { get; }

        public QueryValidationException(List<string> errors) : base("Query validation failed.")
        {
            Errors = errors;
        }
    }
}
