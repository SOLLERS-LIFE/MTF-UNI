using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.Reflection;
using System.Collections.ObjectModel;

using StackExchange.Redis;

using MTF.Areas.Utilities;
using MTF.Areas.Identity.Services.UsersActivity;
using MTF.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MTF
{
    // All parameters needed not once (obtained from correspondent
    // entries in appsettings.json
    public enum MainRetCodes
    {
        OK = 0,
        DBsSeedingProblem = -1,
        Shutdown = -2,
        Restart = -3
    }
    public class combinedFiltersTemplates
    {
        public string EventsLog { get; set; }
    }
    public static class GlobalParameters
    {
        public static int MainRetCode { get; set; } = 0;
        public static string AppIdent { get; set; }
        public static bool _isDevelopment { get; set; }
        private static ILoggerFactory _loggerFactory { get; set; }
        public static ILogger CreateLogger<T>() => _loggerFactory.CreateLogger<T>();
        public static ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);
        public static void setLoggerFactory(ILoggerFactory lf)
        {
            _loggerFactory = lf;
        }

        // Trick to find if in migration routins
        // or any other external actions
        public static bool IsStartedWithMain { get; set; } = false;

        public static string NavbarHeight { get; set; }
        public static string tableHeadAndPagingColor { get; set; }

        public static int DefaultPageSize { get; set; }
        public static List<SelectListItem> AvailablePageSizes { get; set; }
        public static List<SelectListItem> AvailableColorModels { get; set; }
        public static string _defaultColorModel { get; set; }
        public static string TeamNameAlias { get; set; }
        public static string TeamsNameAlias { get; set; }
        public static string MarkNameAlias { get; set; }
        public static string MarksNameAlias { get; set; }
        public static string BusinessStructureAlias { get; set; }

        // set by UAStore
        public static int ClientPageReloadInterval { get; set; } // mins

        // sets by services itself
        public static UAStore _UAStore { get; set; } // Unfortunetaly, it is impossible to use injection
                                                     // throwgh constructor - environment try to create
                                                     // new instance
        public static TechTickle _TechTickle { get; set; }

        public static ConnectionMultiplexer _redis { get; set; }

        // sets by CommonIdentInitializer
        public static string _sus_Id { get; set; } // Super Users role guid
        public static string _sus_Name { get; set; } // Super Users role name
        public static string _everyone_Id { get; set; } // Everyone role guid
        public static string _everyone_Name { get; set; } // Super Users role name
        public static string _testers_Id { get; set; } // Testers role guid
        public static string _testers_Name { get; set; } // Testers Users role name

        public static string _ConnectionStringsNamesPostfix { get; set; }

        public static string _appDB_Path { get; set; }
        public static int _appDB_ConnectionTimeout { get; set; }

        public static AssemblyInfo _appInfo { get; set; }

        public static combinedFiltersTemplates _combinedFiltersTemplates { get; set; }
        public static string _mainTitle { get; set; }
        public static string _baseColor { get; set; }
        public static int _sidenavTransitionDuration { get; set; }
        public static int _sidenavWidth { get; set; }
        public static int _medianLine { get; set; }
        public static Boolean _DisplayConfirmAccountLink { get; set; }

        public static int _serverAndClientTimeout { get; set; }
        public static int _serverAndClientkeepAliveInterval { get; set; }

        public static string _systemClosedButSUS { get; set; }
        public static string _announcement { get; set; }
        public static DateTime _annDate { get; set; }
        public static string _HeadPassword { get; set; }

        public static string replaceAppHeadPassword(IConfiguration cnf, bool _isProduction, string _ss)
        {
            if (_isProduction)
            {
                _HeadPassword = cnf.GetSection("secrets").GetValue<string>("AppHeadPassword", "placeYourPasswordHere");
            }
            else
            {
                _HeadPassword = cnf["secrets:AppHeadPassword"];
            }

            return _ss.Replace("AppHeadPassword", _HeadPassword);
        }

        public static void Fulfill(IConfiguration configuration,
                                   IWebHostEnvironment env
                                  )
        {
            NavbarHeight = configuration.GetSection("UIParameters").GetValue<string>("NavbarHeight", "2.7em");
            tableHeadAndPagingColor = configuration.GetSection("UIParameters").GetValue<string>("TableHeadAndPagingColor", "rgb(220 244 255)");
            DefaultPageSize = configuration.GetSection("UIParameters").GetValue<int>("DefaultPageSize", 25);
            _mainTitle = configuration.GetSection("UIParameters").GetValue<string>("mainTitle", "MTF v.2.1.0");
            _baseColor = configuration.GetSection("UIParameters").GetValue<string>("BaseColor", "#0729c2");
            _sidenavTransitionDuration = configuration.GetSection("UIParameters").GetValue<int>("sidenav-transition-duration", 0);
            _sidenavWidth = configuration.GetSection("UIParameters").GetValue<int>("sidenav-width", 240);
            _medianLine = configuration.GetSection("UIParameters").GetValue<int>("median-line", 900);
            _DisplayConfirmAccountLink = configuration.GetSection("IdentityMailSender").GetValue<bool>("DisplayConfirmAccountLink", false);
            AvailablePageSizes =
                new List<SelectListItem>(new[] { new SelectListItem { Value = "3", Text = "3"},
                                                 new SelectListItem { Value = "5", Text = "5"},
                                                 new SelectListItem { Value = "10", Text = "10"},
                                                 new SelectListItem { Value = "15", Text = "15"},
                                                 new SelectListItem { Value = "25", Text = "25"},
                                                 new SelectListItem { Value = "50", Text = "50"},
                                                 new SelectListItem { Value = "100", Text = "100"},
                                                 new SelectListItem { Value = "250", Text = "250"}
                                               }
                                        );
            AvailableColorModels =
                new List<SelectListItem>(new[] { 
                                                 new SelectListItem { Value = "mdb.min.css", Text = "Traditional Light"},
                                                 new SelectListItem { Value = "mdb.dark.min.css", Text = "Traditional Dark"},
                                                 new SelectListItem { Value = "mdb.navy.light.min.css", Text = "Navy Light"},
                                                 new SelectListItem { Value = "mdb.navy.dark.min.css", Text = "Navy Dark"},
                                                 new SelectListItem { Value = "mdb.red.light.min.css", Text = "Red Light"},
                                                 new SelectListItem { Value = "mdb.red.dark.min.css", Text = "Red Dark"},
                                                 new SelectListItem { Value = "mdb.gold.light.min.css", Text = "Gold Light"},
                                                 new SelectListItem { Value = "mdb.gold.dark.min.css", Text = "Gold Dark"}
                                               }
                                        );
            _defaultColorModel = configuration.GetSection("UIParameters").GetValue<string>("DefaultColorModel", "mdb.dark.min.css");
            TeamNameAlias = configuration.GetSection("UIParameters").GetValue<string>("TeamNameAlias", "Team");
            TeamsNameAlias = configuration.GetSection("UIParameters").GetValue<string>("TeamsNameAlias", "Teams");
            MarkNameAlias = configuration.GetSection("UIParameters").GetValue<string>("MarkNameAlias", "Mark");
            MarksNameAlias = configuration.GetSection("UIParameters").GetValue<string>("MarksNameAlias", "Marks");
            BusinessStructureAlias = configuration.GetSection("UIParameters").GetValue<string>("BusinessStructureAlias", "Business Structure");
            _ConnectionStringsNamesPostfix = "";
            
            _appDB_Path = replaceAppHeadPassword(
                configuration,
                !env.IsDevelopment(),
                configuration.GetConnectionString("AppDBPath"+ _ConnectionStringsNamesPostfix)
                );
            _appDB_ConnectionTimeout = configuration.GetSection("applicationDB").GetValue<int>("connectionTimeout", 5); ;

            _appInfo = new AssemblyInfo();

            _combinedFiltersTemplates = new combinedFiltersTemplates();
            _combinedFiltersTemplates.EventsLog = configuration.GetSection("combinedFiltersTemplates").GetValue<string>("EventsLog", "appIdent like *\n");

            _serverAndClientTimeout = configuration.GetSection("UAStore").GetValue<int>("serverAndClientTimeout", 180);
            _serverAndClientkeepAliveInterval = configuration.GetSection("UAStore").GetValue<int>("serverAndClientkeepAliveInterval", 90);
        }
    }
}
