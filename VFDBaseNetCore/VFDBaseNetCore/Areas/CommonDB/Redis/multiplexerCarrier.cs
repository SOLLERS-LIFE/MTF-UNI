using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;

using StackExchange.Redis;

using MTF.Utilities.Services;
using MTF.Utilities;

namespace MTF.Areas.CommonDB.Redis
{
    public class RedisOptions
    {
        public string optionString { get; set; }
    }
    public class multiplexerCarrier : ITender
    {
        private ILogger<multiplexerCarrier> _logger { get; set; }
        private RedisOptions _options { get; set; }
        public multiplexerCarrier(ILogger<multiplexerCarrier> logger,
                                  IOptions<RedisOptions> options
            )
        {
            _logger = logger;
            _options = options.Value;
        }
            public async Task WarmingUp(CancellationToken stoppingToken)
        {
            try
            {
                var options = ConfigurationOptions.Parse(_options.optionString);
                GlobalParameters._redis = ConnectionMultiplexer.Connect(options);
                _logger.LogInformation($"Redis client initialisation successful.");

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Redis client initialisation failed: " + ex.Message);
            }
        }
        public async Task IndependentFlight(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                   await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
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
        public async Task Landing()
        {
            _logger.LogInformation($"Landing successful.");
            await Task.CompletedTask;
        }

    }
}
