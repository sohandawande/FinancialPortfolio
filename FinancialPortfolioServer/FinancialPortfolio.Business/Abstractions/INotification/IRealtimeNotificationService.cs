using FinancialPortfolio.Models.Model.Response.User;

namespace FinancialPortfolio.Business.Abstractions.INotification
{
    public interface IRealtimeNotificationService
    {
        Task NotifyPendingUserCreatedAsync(PendingUserResponse user, CancellationToken cancellationToken = default);
    }
}
