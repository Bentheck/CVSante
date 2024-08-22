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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(SendNotifications, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void SendNotifications(object state)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope()) // Create a new scope
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CvsanteContext>(); // Get DbContext from the scope
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>(); // Get HubContext from the scope

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
