using CVSante.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CVSante.Services
{
    // This class is used to send notifications to all connected clients when a new FAQ entry is added
    public class NotificationBackground : IHostedService, IDisposable
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private Timer _timer;
        private readonly ILogger<NotificationBackground> _logger;

        public NotificationBackground(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationBackground> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        // This method is called when the application starts
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendNotifications, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        // This method is called every minute to check if any FAQ entry is new
        private async void SendNotifications(object state)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CvsanteContext>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                    // Query the database to check if any FAQ entry is new
                    var newFaqExists = await dbContext.FAQ.AnyAsync(f => f.IsNew);

                    if (newFaqExists)
                    {
                        await hubContext.Clients.All.SendAsync("ReceiveNewItemNotification");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending notifications.");
            }
        }

        // This method is called when the application stops
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
