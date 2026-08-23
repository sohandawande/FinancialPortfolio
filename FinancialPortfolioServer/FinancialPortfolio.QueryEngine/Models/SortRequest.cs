using FinancialPortfolio.QueryEngine.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Models
{
    public sealed class SortRequest
    {
        public string Field { get; set; } = string.Empty;

        public SortDirection Direction { get; set; }
    }
}
