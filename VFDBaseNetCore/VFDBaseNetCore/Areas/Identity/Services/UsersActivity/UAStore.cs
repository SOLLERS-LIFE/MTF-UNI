using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

using MTF.Utilities;
using MTF.Utilities.Services;
using MTF.Areas.Identity.Data;
using MTF.Areas.ApplicationDB.Data;
using System.Runtime.CompilerServices;

namespace MTF.Areas.Identity.Services.UsersActivity
{
    public class UAStoreOptions
    {
        public int CompromizeConfirmationInterval { get; set; }
        public int ClientPageReloadInterval { get; set; }
    }

    // Main Service Body
    public class UAStore : ActualEntriesStore<UserPDBcontext>, ITender
    {
        private readonly ILogger _logger;
        private readonly UAStoreOptions _options;
        // in accordance with the way UAStore created this instance
        // of UserManager will be alive almost by application shutdown
        public readonly UserManager<CommonUser> _userManager;

        // sould take injection of general context of app db
        // with appsa account in connection string
        public UAStore (ILogger<UAStore> logger,
                        IOptions<UAStoreOptions> options,
                        UserManager<CommonUser> userManager
                       )
        {
            _options = options.Value;
            int v = _options.ClientPageReloadInterval > 0 ? _options.ClientPageReloadInterval : 15;
            _options.ClientPageReloadInterval = v;
            GlobalParameters.ClientPageReloadInterval = _options.ClientPageReloadInterval*60*1000;
            int v1 = _options.CompromizeConfirmationInterval > 0 ? _options.CompromizeConfirmationInterval : 60;
            _options.CompromizeConfirmationInterval = v1;

            _logger = logger;
            _userManager = userManager;

            RegisterActualizationCallback(ActualizationCallback);
        }

        private async Task ActualizationCallback (string uId)
        {
            _logger.LogInformation($"{(await _userManager.FindByIdAsync(uId)).Email} logged in.");
        }

        public async Task WarmingUp(CancellationToken stoppingToken)
        {
            // adding self-reference to static pool
            GlobalParameters._UAStore = this;

            _logger.LogInformation($"UAStore warming up successful.");
            await Task.CompletedTask;
        }
        public async Task Landing()
        {
            List<string> dlt = DeleteAllEntries();

            foreach (var uId in dlt)
            {
                _logger.LogInformation($"{(await _userManager.FindByIdAsync(uId)).Email} logged out.");
            }

            _logger.LogInformation($"UAStore landing successful.");
        }

        public async Task IndependentFlight(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    List<string> dlt = await DeleteCompromizedEntries(TimeSpan.FromSeconds(_options.CompromizeConfirmationInterval / 2));
                    foreach (var uId in dlt)
                    {
                        _logger.LogInformation($"{(await _userManager.FindByIdAsync(uId)).Email} logged out.");
                    }
                    await Task.Delay(TimeSpan.FromSeconds(_options.CompromizeConfirmationInterval), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().ToString() == "System.Threading.Tasks.TaskCanceledException")
                {
                    _logger.LogWarning($"Exception '{ex.Message}' happend.");
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                await Landing();
            }
        }
    }
}
