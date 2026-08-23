using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Models
{
    public sealed class QueryRequest
    {
        public string? GlobalSearch { get; set; }

        public List<FilterRequest>? Filters { get; set; }

        public List<SortRequest>? Sorts { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
