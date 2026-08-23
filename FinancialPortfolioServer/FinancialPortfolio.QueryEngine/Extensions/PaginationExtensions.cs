using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Extensions
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }
    }
}
