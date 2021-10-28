// Service to execute every 1 min.
// This is a base for various business service procedures
// for the application, including backup and so on.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using MTF.Utilities;
using MTF.Areas.CommonDB.Data;
using MTF.Areas.Identity.Data;
using MTF.Areas.Identity.Authorization;
using MTF.Utilities.Services;
using MTF.Hubs;

namespace MTF.Services
{
    public class TechTickleOptions
    {
        public int Period { get; set; }
    }
    public class TechTickleOperation
    {
        public string Command { get; set; }
        public string UserName { get; set; }
        public string Operand { get; set; }
        public TechTickleOperation(string cmd, string un="", string op1="")
        {
            Command = cmd;
            UserName = un;
            Operand = op1;
        }
    }

    // Actual service - we can inject different objects in constructor
    public class TechTickle : DelayedList<TechTickleOperation>, ITender
    {
        private readonly ILogger _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IHubContext<UsersInterconnectHub> _hub;
        private readonly IHttpContextAccessor _httpAccr; // how to obtain current user in middleware
        private readonly CancellationToken _cancellationToken;

        private readonly CommonDB_Context _cntx;
        private readonly UserManager<CommonUser> _userManager;
        private readonly TechTickleOptions _options;

        // private int _numSteps = 0;

        public TechTickle(ILogger<TechTickle> logger,
                          IHostApplicationLifetime appLifetime,
                          IHttpContextAccessor httpAccr, // to obtain current http context
                          UserManager<CommonUser> userManager,
                          IHubContext<UsersInterconnectHub> hub,

                          CommonDB_Context cntx,
                          IOptions<TechTickleOptions> options)
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _hub = hub;
            _httpAccr = httpAccr;
            _cancellationToken = appLifetime.ApplicationStopping;

            _cntx = cntx;
            _userManager = userManager;
            _options = options.Value;
        }

        // Require Shutdown
        public async Task RequireShutdown (int toMins, string msg, MainRetCodes rc)
        {
            toMins = toMins < 0 ? 0 : toMins;
            GlobalParameters.MainRetCode = (int)rc;

            CommonUser user = await verifyPermission(AuthOperations.NoRestrictions);
            if (user!=null)
            {
                await SendBroadCast(msg, user.UserName);
                await DLPut(new TechTickleOperation("shutdown",user.UserName), toMins);

                _logger.LogWarning($"Restart requested by user {httpUser().Identity.Name}.");
            }
        }
        // Require BroadCast
        public async Task RequireBroadcast(int toMins, string msg)
        {
            toMins = toMins < 0 ? 0 : toMins;

            CommonUser user = await verifyPermission(AuthOperations.CanBroadcast);
            if (user != null)
            {
                await DLPut(new TechTickleOperation("broadcast", user.UserName, msg), toMins);
            }
        }

        protected async Task ProceedCommands()
        {
            List<TechTickleOperation> lst = await DLGet();
            foreach(TechTickleOperation el in lst)
            {
                switch (el.Command)
                {
                    case "shutdown":
                        ActualShutdown();
                        break;
                    case "broadcast":
                        await SendBroadCast(el.Operand, el.UserName);
                        break;
                    default:
                        break;
                }
            }
        }

        // Prolog immediately after services ready
        public async Task WarmingUp (CancellationToken stoppingToken)
        {
            // adding self-reference to static pool
            GlobalParameters._TechTickle = this;

            _logger.LogInformation($"WarmingUp successful.");
            await Task.CompletedTask;
        }
        // Epilog during host shutdown
        public async Task Landing()
        {
            _logger.LogInformation($"Landing successful.");
            await Task.CompletedTask;
        }

        // Body of service itself
        public async Task IndependentFlight(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProceedCommands();

                    await Task.Delay(TimeSpan.FromSeconds(_options.Period), stoppingToken);
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

        protected async Task SendBroadCast(string pmsg, string un)
        {

            await _hub.Clients.All.SendAsync("BroadCast", new { msg = pmsg });
            _logger.LogWarning($"BroadCast sended by user {un}: {pmsg}");
        }

        protected void ActualShutdown()
        {
            _logger.LogWarning($"Actual Restart started.");
            _appLifetime.StopApplication();
        }

        // Obtain current ClaimsPrincipial of http requests
        protected ClaimsPrincipal httpUser()
        {
            return _httpAccr.HttpContext.User;
        }

        protected async Task<CommonUser> verifyPermission(IAuthorizationRequirement req)
        {
            var user = await _userManager.GetUserAsync(httpUser());
            if (user == null)
            {
                return null;
            }
            var _authorizationService = _httpAccr.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
            if (
                (await _authorizationService.AuthorizeAsync(httpUser(),
                                                            user.Id,
                                                            req
                                                           )
                ).Succeeded)
            {
                return user;
            }
            return null;
        }
    }
}