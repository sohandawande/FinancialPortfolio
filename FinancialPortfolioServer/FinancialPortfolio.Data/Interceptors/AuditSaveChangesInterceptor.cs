using FinancialPortfolio.Data.Common.Base;
using FinancialPortfolio.Models.Abstractions.ICurrentUser;
using FinancialPortfolio.Models.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinancialPortfolio.Data.Interceptors
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly TimeProvider _timeProvider;
        private readonly ICurrentUserService _currentUserService;

        public AuditSaveChangesInterceptor(TimeProvider timeProvider, ICurrentUserService currentUserService)
        {
            _timeProvider = Guard.AgainstNull(timeProvider, nameof(timeProvider));
            _currentUserService = Guard.AgainstNull(currentUserService, nameof(currentUserService));
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContext? context)
        {
            if (context is null) return;

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var currentUserId = _currentUserService.UserId;

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = utcNow;
                        entry.Entity.ModifiedDate = utcNow;
                        entry.Entity.CreatedBy = currentUserId;
                        entry.Entity.ModifiedBy = currentUserId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedDate = utcNow;
                        entry.Entity.ModifiedBy = currentUserId;
                        break;
                }
            }
        }
    }
}