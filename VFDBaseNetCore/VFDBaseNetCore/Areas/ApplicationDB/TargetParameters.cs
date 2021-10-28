using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MTF.Areas.ApplicationDB
{
    public static class TargetParameters
    {
        public static void Fulfill (IConfiguration configuration,
                                   IWebHostEnvironment env
            )
        {
        }
    }
}
