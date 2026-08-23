using FinancialPortfolio.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Business.Common.Helpers
{
    public static class UserCodeGeneratorHelper
    {
        public const string Prefix = "FP";
        public const int Digits = 4;

        /// <summary>
        /// Next code: FP0001, FP0002, ... based on max existing FP#### in AppUsers.
        /// </summary>
        public static async Task<string> NextAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            var codes = await context.AppUsers
                .AsNoTracking()
                .Where(x => x.UserCode.StartsWith(Prefix))
                .Select(x => x.UserCode)
                .ToListAsync(cancellationToken);

            var max = 0;
            foreach (var code in codes)
            {
                if (code.Length <= Prefix.Length) continue;
                if (int.TryParse(code.AsSpan(Prefix.Length), out var n) && n > max)
                    max = n;
            }

            return $"{Prefix}{(max + 1).ToString().PadLeft(Digits, '0')}";
        }
    }
}
