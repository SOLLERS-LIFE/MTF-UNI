using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;

using MTF.Hubs;

namespace MTF.Utilities.Services
{
    // Ship's tender interface for using with Mother-Ship
    public interface ITender
    {
        public Task WarmingUp(CancellationToken stoppingToken);
        public Task IndependentFlight(CancellationToken stoppingToken);
        public Task Landing();
    }
    // Mother-Ship itself
    public class MotherShip : BackgroundService
    {
        private readonly ILogger<MotherShip> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IHubContext<UsersInterconnectHub> _hub;

        public IServiceProvider _services { get; }

        public MotherShip(IServiceProvider services,
                          ILogger<MotherShip> logger,
                          IHostApplicationLifetime appLifetime,
                          IHubContext<UsersInterconnectHub> hub)
        {
            _services = services;
            _logger = logger;
            _appLifetime = appLifetime;
            _hub = hub;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            await base.StartAsync(cancellationToken);
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"StopAsync has been called.");

            await base.StopAsync(stoppingToken);
        }

        private void OnStarted()
        {
            _logger.LogInformation($"OnStarted has been called.");
            // Perform post-startup activities here
        }
        private void OnStopping()
        {
            // Notify all connected browsers
            SendBroadCast("System has gone offline. Enjoy yourself!", "system").Wait();

            _logger.LogInformation($"OnStopping has been called.");
        }
        private void OnStopped()
        {
            _logger.LogInformation($"OnStopped has been called.");
            // Perform post-stopped activities here
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                $"MotherShip.ExecuteAsync has been called.");

            await Runway(stoppingToken);
        }

        protected async Task Runway(CancellationToken stoppingToken)
        {
            try
            {
                using (IServiceScope scope = _services.CreateScope())
                {
                    IEnumerable<ITender> srvs =
                        scope.ServiceProvider
                            .GetServices<ITender>();

                    await Task.WhenAll(srvs.Select(t => t.WarmingUp(stoppingToken)));
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    await Task.WhenAll(srvs.Select(t => t.IndependentFlight(stoppingToken)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception '{ex.Message}' happend.");
                _logger.LogError($"Application restart requested because of ITender has crushed.");
                GlobalParameters.MainRetCode = (int)MainRetCodes.Restart;
                _appLifetime.StopApplication();
            }
        }

        protected async Task SendBroadCast(string pmsg, string un)
        {
            try
            {
                await _hub.Clients.All.SendAsync("BroadCast", new { msg = pmsg });
                _logger.LogWarning($"BroadCast sended by user {un}: {pmsg}");
            }
            catch { }
        }
    }
}
