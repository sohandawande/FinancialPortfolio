using FinancialPortfolio.Data.Entities.AppUser;
using FinancialPortfolio.Data.Entities.Portfolio;
using FinancialPortfolio.Data.Entities.RefreshToken;
using FinancialPortfolio.Data.Entities.Stock;
using FinancialPortfolio.Data.Entities.SystemLog;
using FinancialPortfolio.Data.Identity;
using FinancialPortfolio.Data.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinancialPortfolio.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly AuditSaveChangesInterceptor _auditInterceptor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, AuditSaveChangesInterceptor auditInterceptor) : base(options)
        {
            _auditInterceptor = auditInterceptor;
        }

        public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
        public DbSet<StockEntity> Stocks => Set<StockEntity>();
        public DbSet<StockDetailEntity> StockDetails => Set<StockDetailEntity>();
        public DbSet<AppUserEntity> AppUsers => Set<AppUserEntity>();
        public DbSet<SystemLogEntity> SystemLogs => Set<SystemLogEntity>();
        public DbSet<SystemLogDetailEntity> SystemLogDetails => Set<SystemLogDetailEntity>();
        public DbSet<PortfolioEntity> Portfolios => Set<PortfolioEntity>();
        public DbSet<PortfolioStockHoldEntity> PortfolioStockHolds => Set<PortfolioStockHoldEntity>();
        public DbSet<PortfolioStockSoldEntity> PortfolioStockSolds => Set<PortfolioStockSoldEntity>();
        public DbSet<PortfolioStockDividendEntity> PortfolioStockDividends => Set<PortfolioStockDividendEntity>();
        public DbSet<PortfolioMutualFundEntity> PortfolioMutualFunds => Set<PortfolioMutualFundEntity>();
        public DbSet<PortfolioFixedDepositEntity> PortfolioFixedDeposits => Set<PortfolioFixedDepositEntity>();
        public DbSet<PortfolioRecurringDepositEntity> PortfolioRecurringDeposits => Set<PortfolioRecurringDepositEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            //modelBuilder.ApplyConfiguration(new SystemLogConfiguration());

            // Automatically registers both StockConfiguration and StockDetailConfiguration
            //modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
