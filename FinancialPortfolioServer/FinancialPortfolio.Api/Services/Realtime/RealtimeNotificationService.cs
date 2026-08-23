using FinancialPortfolio.Api.Hubs;
using FinancialPortfolio.Business.Abstractions.INotification;
using FinancialPortfolio.Models.Common.Utilities;
using FinancialPortfolio.Models.Model.Response.User;
using Microsoft.AspNetCore.SignalR;

namespace FinancialPortfolio.Api.Services.Realtime
{
    public sealed class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public RealtimeNotificationService(IHubContext<NotificationHub> hub)
        {
            _hub = Guard.AgainstNull(hub, nameof(hub));
        }

        public Task NotifyPendingUserCreatedAsync(PendingUserResponse user, CancellationToken cancellationToken = default)
        {
            return _hub.Clients.Group(NotificationHub.AdminGroup).SendAsync("PendingUserCreated", user, cancellationToken);
        }
    }
}
