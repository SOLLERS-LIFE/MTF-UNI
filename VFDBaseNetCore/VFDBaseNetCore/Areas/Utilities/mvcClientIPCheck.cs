using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
// using Microsoft.AspNetCore.HttpOverrides;

// using nuget pkg IPNetwork2

namespace MTF.Utilities
{
    public class IpSafeList
    {
        public Boolean Enabled { get; set; }
        public string IpAddresses { get; set; }
        public string IpNetworks { get; set; }
    }

    // for clear controllers
    // https://docs.microsoft.com/en-us/aspnet/core/security/ip-safelist?view=aspnetcore-5.0
    // https://codeburst.io/implementing-and-testing-ip-safelists-in-asp-net-core-dbd9e6f4b696
    public class ClientIpCheckActionFilter : ActionFilterAttribute
    {
        private readonly ILogger<ClientIpCheckActionFilter> _logger;
        private readonly IpSafeList _ipSafeList;

        public ClientIpCheckActionFilter(IpSafeList safeList, ILogger<ClientIpCheckActionFilter> logger)
        {
            _ipSafeList = safeList;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_ipSafeList.Enabled)
            {
                var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
                if (remoteIp == null)
                {
                    throw new ArgumentException("Remote IP is NULL, may due to missing ForwardedHeaders.");
                }

                _logger.LogDebug($"Request from IP: {remoteIp}");
                if (!IsFromSafeList(_ipSafeList, remoteIp))
                {
                    _logger.LogWarning($"Request from unsafe IP: {remoteIp}");
                    context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        private static bool IsFromSafeList (IpSafeList safeList,
                                           IPAddress remoteIp
                                           )
        {
            if (!safeList.Enabled)
            {
                return true;
            }
            List<IPAddress> ipAddresses = safeList.IpAddresses.Split(';').Select(IPAddress.Parse).ToList();
            List<IPNetwork> ipNetworks = safeList.IpNetworks.Split(';').Select(IPNetwork.Parse).ToList();

            if (remoteIp == null)
            {
                throw new ArgumentException("Remote IP is NULL, may due to missing ForwardedHeaders.");
            }

            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }

            if (!ipAddresses.Contains(remoteIp) && !ipNetworks.Any(x => x.Contains(remoteIp)))
            {
                return false;
            }

            return true;
        }
    }

    // For razor pages
    // https://docs.microsoft.com/en-us/aspnet/core/razor-pages/filter?view=aspnetcore-5.0
    
}
