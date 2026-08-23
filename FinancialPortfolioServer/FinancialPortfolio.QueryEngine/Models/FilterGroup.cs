using FinancialPortfolio.QueryEngine.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Models
{
    public sealed class FilterGroup
    {
        public List<FilterRequest> Filters { get; set; } = new();

        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;
    }
}
