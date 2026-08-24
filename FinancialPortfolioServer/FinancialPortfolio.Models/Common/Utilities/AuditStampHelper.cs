namespace FinancialPortfolio.Models.Common.Utilities
{
    /// <summary>
    /// Shared audit id for CreatedBy/ModifiedBy. Those columns are required on most tables.
    /// Prefer the signed-in user, then an existing stamp, then 0 for background work.
    /// Never returns null.
    /// </summary>
    public static class AuditStampHelper
    {
        public static long Resolve(long currentUserId, long? existing = null)
        {
            if (currentUserId > 0)
                return currentUserId;
            if (existing is > 0)
                return existing.Value;
            return 0;
        }
    }
}
