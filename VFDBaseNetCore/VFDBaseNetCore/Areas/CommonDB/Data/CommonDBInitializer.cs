using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.EntityFrameworkCore;

using MTF.Areas.CommonDB.Models;

namespace MTF.Areas.CommonDB.Data
{
    public static class CommonDBInitializer
    {
        public static async Task<int> DoIt (
                                            IConfiguration config,
                                            IServiceProvider services 
                                           )
        {
            var CDB = services.GetService<CommonDB_Context>();

            var ann = (await CDB._announcements.FromSqlRaw("call cdb.sa_get_latest_announcement;")
                                                            .AsNoTracking()
                                                            .ToListAsync())
                                                            .FirstOrDefault();
            if (ann == null)
            { GlobalParameters._announcement = ""; }
            else
            { 
                GlobalParameters._announcement = ann.content;
                GlobalParameters._annDate = ann.DateIn;
            }

            ann = (await CDB._announcements.FromSqlRaw("call cdb.sa_get_latest_loginsdisabler;")
                                                            .AsNoTracking()
                                                            .ToListAsync())
                                                            .FirstOrDefault();
            if (ann == null)
            { GlobalParameters._systemClosedButSUS = ""; }
            else
            {
                GlobalParameters._systemClosedButSUS = ann.content;
            }
            
            return 0;
        }
    }
}
