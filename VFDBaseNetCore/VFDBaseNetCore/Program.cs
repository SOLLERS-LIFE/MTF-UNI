using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Server.Kestrel.Core;

using MySqlConnector.Authentication.Ed25519;

using NLog.Web;
using NLog;

using MTF.Areas.Identity.Data;
using MTF.Areas.CommonDB.Data;
using MTF.Utilities.Services;
using MTF.Services;
using MTF.Areas.Identity.Services.UsersActivity;
using MTF.Areas.CommonDB.Redis;

namespace MTF
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Trick to find if in migration routins
            // or any other external actions
            GlobalParameters.IsStartedWithMain = true;

            // to enable new user verification
            // with MySqlConnector.Authentication.Ed25519 NuGet Package
            Ed25519AuthenticationPlugin.Install();

            var logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                var config = host.Services.GetRequiredService<IConfiguration>();

                GlobalParameters.AppIdent = config.GetSection("Logging").GetValue<string>("AppIdent", "MTF App");
                GlobalDiagnosticsContext.Set("AppIdent", GlobalParameters.AppIdent);

                //var loggersFactory = host.Services.GetRequiredService<ILoggerFactory>();
                //var logger = loggersFactory.CreateLogger("Program.Main");

                var scope = host.Services.CreateScope();
                var services = scope.ServiceProvider;

                var context = services.GetService<CommonIdent>();
                // If we have any pendibf migrations - apply them
                // context.Database.Migrate();

                RoleManager<CommonRole> roleManager = services.GetService<RoleManager<CommonRole>>();
                UserManager<CommonUser> userManager = services.GetService<UserManager<CommonUser>>();

                // alternative way to obtain sensitive data - use dotnet user-secrets set <name> <value>
                // then use the configuration provider (obtain it via injection see Privacy page class
                // var suPassword = config["su_pwd"];

                // Here we have IHost host IConfiguration config IServiceProvider services
                // And we can pass them to initializers
                CommonIdentInitializer.DoIt(config, roleManager, userManager, services).Wait();
                CommonDBInitializer.DoIt(config, services).Wait();

                host.Run();
            }
            catch (Exception ex)
            {
                logger.Error($"Unhandled xception '{ex.Message}' happend.");
                logger.Error($"Application restart requested.");
                GlobalParameters.MainRetCode = (int)MainRetCodes.Restart;
                // throw;
            }
            finally
            {
                logger.Warn($"MTF exiting with exit code {GlobalParameters.MainRetCode}.");
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                NLog.LogManager.Shutdown();
            }

            return GlobalParameters.MainRetCode;
        }

        protected class kestlerOptions 
        {
            public string certificateFileName { get; set; }
            public string certificatePassword { get; set; }
            public string certificateFileNameProduction { get; set; }
            public string certificatePasswordProduction { get; set; }
            public int httpPort { get; set; }
            public int httpsPort { get; set; }
            public int httpPortProduction { get; set; }
            public int httpsPortProduction { get; set; }
        }
        protected static kestlerOptions KestrelGetOptions ()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("site-kestrel.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"site-kestrel.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                                       optional: true, reloadOnChange: true)
            .Build();

            var certificateSettings = config.GetSection("certificateSettings");
            string certificateFileName = certificateSettings.GetValue<string>("fileName");
            string certificatePassword = GlobalParameters._HeadPassword; // certificateSettings.GetValue<string>("password");
            string certificateFileNameProduction = certificateSettings.GetValue<string>("fileNameProduction");
            string certificatePasswordProduction = GlobalParameters._HeadPassword; // certificateSettings.GetValue<string>("passwordProduction");
            var portsSettings = config.GetSection("portsSettings");
            int httpPort = portsSettings.GetValue<int>("httpPort",5000);
            int httpsPort = portsSettings.GetValue<int>("httpsPort", 5001);
            int httpPortProduction = portsSettings.GetValue<int>("httpPortProduction", 80);
            int httpsPortProduction = portsSettings.GetValue<int>("httpsPortProduction", 443);
            var exchangeSettings = config.GetSection("exchangeSettings");
            
            return new kestlerOptions
                        { certificateFileName = certificateFileName,
                          certificatePassword = certificatePassword,
                          certificateFileNameProduction = certificateFileNameProduction,
                          certificatePasswordProduction = certificatePasswordProduction,
                          httpPort = httpPort,
                          httpsPort = httpsPort,
                          httpPortProduction = httpPortProduction,
                          httpsPortProduction = httpsPortProduction
            };
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.UseEnvironment("Production")
                //.UseEnvironment("Development")
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                })
                .ConfigureLogging(logging =>
                {
                    // Explicity configure logging providers
                    logging.ClearProviders();  // Cleare default
                    //logging.AddConsole();
                    //logging.AddDebug();
                    logging.AddNLog("nlog.config");
                    
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                //.UseNLog() // NLog: Setup NLog for Dependency injection
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(true); // catch exceptions during startup, not quit the app
                    webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                    webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(30));
                    webBuilder.UseKestrel(
                                            (hostContext, options) =>
                                            {
                                                var o = KestrelGetOptions();
                                                if (hostContext.HostingEnvironment.IsDevelopment())
                                                {
                                                    options.AddServerHeader = false;
                                                    options.Listen(IPAddress.Any, o.httpPort,
                                                        listenOptions => {
                                                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                                                        }
                                                    );
                                                    options.Listen(IPAddress.Any, o.httpsPort, 
                                                        listenOptions => {
                                                            listenOptions.UseHttps(o.certificateFileName,
                                                                                   o.certificatePassword);
                                                        }
                                                    );
                                                }
                                                else
                                                {
                                                    options.AddServerHeader = false;
                                                    options.Listen(IPAddress.Any, o.httpPortProduction,
                                                        listenOptions => {
                                                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                                                        }
                                                    );
                                                    options.Listen(IPAddress.Any, o.httpsPortProduction,
                                                        listenOptions => {
                                                            listenOptions.UseHttps(o.certificateFileNameProduction,
                                                                                   o.certificatePasswordProduction);
                                                                         }
                                                                  );
                                                }
                                            }
                                         );
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    int shto = hostContext.Configuration.GetSection("AppBehaviour").GetValue<int>("ShutdownTimeout", 30);
                    services.Configure<HostOptions>(
                                                     opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(shto)
                                                   );
                    services.Configure<KestrelServerOptions>(
                                                                hostContext.Configuration.GetSection("Kestrel")
                                                            );

                    // Background Services Facility
                    // Use here instead of startup class to guaranty execution
                    // in the moment the default webhost configured and basic
                    // services are ready to injection
                    services.AddHostedService<MotherShip>();
                    // All tender services to run with mother-ship
                    services.AddScoped<ITender, TechTickle>();
                    services.Configure<TechTickleOptions>(
                                                          hostContext.Configuration.GetSection("TechTickle")
                                                         );
                    services.AddScoped<ITender, UAStore>();
                    services.Configure<UAStoreOptions>(
                                                        hostContext.Configuration.GetSection("UAStore")
                                                      );
                    
                    // adding some useful services
                    services.AddScoped<ITender, multiplexerCarrier>();
                    services.Configure<RedisOptions>(
                                                       hostContext.Configuration.GetSection("Redis")
                                                     );
                    services.AddTransient<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();
                });
    }
}
