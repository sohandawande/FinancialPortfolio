using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Models
{
    public sealed class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
