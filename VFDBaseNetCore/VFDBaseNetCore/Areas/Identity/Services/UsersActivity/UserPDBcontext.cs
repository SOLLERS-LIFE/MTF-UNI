using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

using MTF.Utilities;
using MTF.Utilities.Services;
using MTF.Areas.Identity.Data;
using MTF.Areas.ApplicationDB.Data;
using System.Runtime.CompilerServices;

namespace MTF.Areas.Identity.Services.UsersActivity
{

    public class UserPDBcontext : AESelem
    {
        protected BasicFactory<AppDB_Context, DbContextOptions<AppDB_Context>> _dbFactory = null;
        private bool _inited { get; set; } = false;

        private string _uId { get; set; }
        private string _appDBPsw { get; set; }

        private object _locker = new object();
        private bool _disposed = false;

        // Here should be structures of class factory
        // with two quewes - one for ready dbc, another one for leased
        
        public UserPDBcontext(string uId)
        // cannot do a lot because of execution inside lock statement
        {
            _uId = uId;
        }
        public UserPDBcontext() { }

        // Action methods
        public async Task init(object prms)
        // should be very carefull with locking
        {
            var pt = prms.GetType().GetProperties();
            if (
                (pt.Length != 2) ||
                (pt[0].Name != "userManager") ||
                (pt[0].PropertyType.Name != typeof(UserManager<CommonUser>).Name) ||
                (pt[1].Name != "appDBContext") ||
                (pt[1].PropertyType.Name != typeof(AppDB_Context).Name)
               )
            {
                throw new Exception("UserPDBcontext.init parameters mismatch.");
            }

            UserManager<CommonUser> _UserManager = (UserManager<CommonUser>)(pt[0].GetValue(prms, null));

            // should fullfill the _appDBPsw
            var usr = await _UserManager.FindByIdAsync(_uId);
            if (
                (usr == null) ||
                (usr.Id != _uId)
               )
            {
                throw new Exception("UserPDBcontext.init User Manager doesnt work.");
            }

            bool was_inited = true;
            lock (_locker)
            {
                was_inited = _inited;
                if (!_inited)
                {
                    _appDBPsw = usr.AppPswd ?? "55555";
                    _inited = true;
                }
            }

            if (
                (was_inited) &&
                ((usr.AppPswd ?? "55555") == "55555")
               )
            {
                throw new Exception("UserPDBcontext.init Mark as inited but AppPsw is not defined.");
            }

            if (!was_inited)
            // Can perform som asinc actions with administrative
            // appdb context
            {
                AppDB_Context adbc = (AppDB_Context)(pt[1].GetValue(prms, null));
                DbContextWithScalarReturn.ScalarReturn res =
                                         await adbc.getScalarValue("call appdb.sa_create_appuser",
                                                                    _uId,
                                                                    _appDBPsw,
                                                                    "appuser"
                                                                    );
                switch (res.RetValueInt)
                {
                    case 0:
                        // user recreated, password returned
                        // should save password in Identity record
                        _appDBPsw = res.RetValueString;
                        usr.AppPswd = res.RetValueString;
                        await _UserManager.UpdateAsync(usr);

                        break;
                    case 1:
                        // user exists, password is the same -
                        // nothing to do
                        break;
                    default:
                        // error inside mariadb procedure - crush the app!
                        // What can we do?
                        throw new Exception("UserPDBcontext.init stored procedure syntax error.");
                }

                var optionsBuilder = new DbContextOptionsBuilder<AppDB_Context>();
                var connectionString = GlobalParameters._appDB_Path + @$"user = {_uId}; password = {_appDBPsw};";
                optionsBuilder.UseMySql(
                                         connectionString,
                                         //GlobalParameters._appDB_Path+
                                         //@$"user = {_uId}; password = {_appDBPsw};",
                                         ServerVersion.AutoDetect(connectionString)
                                         //mySqlOptions => mySqlOptions.ServerVersion(new Version(10, 4, 13), ServerType.MariaDb)
                                       )
                              //.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                              .EnableDetailedErrors(true);
                
                _dbFactory = new BasicFactory<AppDB_Context, DbContextOptions<AppDB_Context>>(optionsBuilder.Options);
            }

            await Task.CompletedTask;
        }

        public async Task<object> get()
        {
                return await _dbFactory.hire();
        }

        public void release(object dbc)
        {
                _dbFactory.release((AppDB_Context)dbc);
        }

        // IDisposable reaalization
        ~UserPDBcontext() => Dispose(false);
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose Factory(ies) here.
                if (_dbFactory!=null)
                {
                    _dbFactory.Dispose();
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}