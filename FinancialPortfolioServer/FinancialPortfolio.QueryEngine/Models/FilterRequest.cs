using FinancialPortfolio.QueryEngine.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Models
{
    public sealed class FilterRequest
    {
        public string Field { get; set; } = string.Empty;

        public FilterOperator Operator { get; set; }

        public string Value { get; set; } = string.Empty;
    }
}
