using System;
using System.Text;
using System.Reflection;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

using WebMarkupMin.AspNetCore5;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNet.Common.UrlMatchers;
using WebMarkupMin.Core;

// from nuget and https://github.com/michaelvs97/AspNetCore.ReCaptcha
using AspNetCore.ReCaptcha;

// using MTF.WEBAPI;

// Pomelo nuget package - both MySQL and MariaDB
// Using MySqlConnector
// using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

// client to k8s instance of https://thecodingmachine.github.io/gotenberg/
// https://github.com/ChangemakerStudios/GotenbergSharpApiClient
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Domain.Settings;

using MTF.Utilities;
using MTF.Areas.Identity.Authorization;
using MTF.Areas.Identity.Data;
using MTF.Areas.CommonDB.Data;
using MTF.Areas.ApplicationDB.Data;
using MTF.Hubs;
using MTF.Services;
using MTF.Areas.Logging.Data;

namespace MTF
{
    public class Startup
    {
        public Startup(IConfiguration configuration,
                       IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
            GlobalParameters.Fulfill(Configuration, env);
            MTF.Areas.ApplicationDB.TargetParameters.Fulfill(Configuration, env);
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment _env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            GlobalParameters._isDevelopment = _env.IsDevelopment();
            // Differences for environments
            if (!_env.IsDevelopment())
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                    options.HttpsPort = Configuration.GetSection("UIParameters").GetValue<int>("HttpsPort", 443);
                });
                services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(365);
                });

                services.AddWebOptimizer(pipeline =>
                {
                    pipeline.MinifyJsFiles("js/**/*.js", "js/*.js", "js/signalr/dist/browser/*.js");
                    pipeline.MinifyCssFiles("css/**/*.css");
                });
            }
            else
            {
                services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = Configuration.GetSection("UIParameters").GetValue<int>("HttpPort", 5001);
                });

                services.AddWebOptimizer(minifyJavaScript: false, minifyCss: false);
            }



            // necessary for k8s with multireplica
            var crt = new X509Certificate2(Configuration.GetSection("DataProtection").GetValue<string>("pfxFileName"),
                                           GlobalParameters.replaceAppHeadPassword(
                                               Configuration,
                                               !_env.IsDevelopment(),
                                               Configuration.GetSection("DataProtection").GetValue<string>("pfxPassword")
                                               )
                                          );
            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(Configuration.GetSection("DataProtection").GetValue<string>("KeyLocation")))
                .ProtectKeysWithCertificate(crt)
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                })
                .SetDefaultKeyLifetime(TimeSpan.FromDays(Configuration.GetSection("DataProtection").GetValue<int>("DefaultKeyLifetime", 600)))
                .SetApplicationName(Configuration.GetSection("Logging").GetValue<string>("AppIdent", "MTF-based App"));

            // Add Identity 
            var connectionString_ci = GlobalParameters.replaceAppHeadPassword(
                Configuration,
                !_env.IsDevelopment(),
                Configuration.GetConnectionString("CommonIdentConnectionMySQL" + GlobalParameters._ConnectionStringsNamesPostfix)
                );
            services.AddDbContext<CommonIdent>(options =>
                options.UseMySql(
                    connectionString_ci,
                    ServerVersion.AutoDetect(connectionString_ci)
                ));

            // add Identity component
            services.AddDefaultIdentity<CommonUser>(options => { options.SignIn.RequireConfirmedAccount = true; } )
                // Enable roles for Identity
                .AddRoles<CommonRole>()
                .AddEntityFrameworkStores<CommonIdent>();
            // configure the interval if security stamp changed
            // then await userManager.UpdateSecurityStampAsync(user)
            // will log off a user immediately from all contexts
            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // enables immediate logout, after updating the user's stat.
                options.ValidationInterval = TimeSpan.Zero;
            });

            // Add facebook authentication
            services.AddAuthentication()
                // .AddFacebook(facebookOptions =>
                //     {
                //         facebookOptions.AppId = Configuration.GetSection("FaceBook").GetValue<string>("AppId", "");
                //         facebookOptions.AppSecret = Configuration.GetSection("FaceBook").GetValue<string>("AppSecret", "");
                //         facebookOptions.AccessDeniedPath = "/Identity/Account/AccessDenied";
                //     })
                .AddJwtBearer(options =>
                    {
                        options.SaveToken = true;
                        options.RequireHttpsMetadata = false;
                        
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ClockSkew = new System.TimeSpan(0, 0, 30),

                            ValidateLifetime = true,

                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidAudience = Configuration["JWT:ValidAudience"],
                            ValidIssuer = Configuration["JWT:ValidIssuer"],

                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                        };

                    })
                ;

            // Adding CommonDB context
            var connectionString_cdb = GlobalParameters.replaceAppHeadPassword(
                Configuration,
                !_env.IsDevelopment(),
                Configuration.GetConnectionString("CommonDBConnectionMySQL" + GlobalParameters._ConnectionStringsNamesPostfix)
                );
            services.AddDbContext<CommonDB_Context>(
                options => options.UseMySql(connectionString_cdb,
                                            //mySqlOptions => mySqlOptions.ServerVersion(new Version(10, 4, 13), ServerType.MariaDb)
                                            ServerVersion.AutoDetect(connectionString_cdb)
                ));
            // Adding LogDB context
            var connectionString_lg = GlobalParameters.replaceAppHeadPassword(
                Configuration,
                !_env.IsDevelopment(),
                Configuration.GetConnectionString("LogDBConnectionMySQL" + GlobalParameters._ConnectionStringsNamesPostfix)
                );
            services.AddDbContext<LogDB_Context>(
                options => options.UseMySql(connectionString_lg,
                                            ServerVersion.AutoDetect(connectionString_lg)
                ));

            // Adding AppDB context with sa priveledge. It's a system context.
            // Users will work under where own MariaDB accounts.
            var connectionString_adb = GlobalParameters.replaceAppHeadPassword(
                Configuration,
                !_env.IsDevelopment(),
                Configuration.GetConnectionString("AppDBConnectionMySQL" + GlobalParameters._ConnectionStringsNamesPostfix)
                );
            services.AddDbContext<AppDB_Context>(
                options => options.UseMySql(connectionString_adb,
                                            ServerVersion.AutoDetect(connectionString_adb)
                                            ));

            services.AddMvc();
            services.AddRazorPages().AddRazorPagesOptions(options =>
            {
                options.RootDirectory = "/Pages";
                options.Conventions.AddPageRoute("/index", "/index.html");
                // https://exceptionnotfound.net/how-to-use-routing-in-asp-net-core-3-0-razor-pages/
                // https://www.learnrazorpages.com/razor-pages/routing
            });

            // Adding recaptcha from https://github.com/michaelvs97/AspNetCore.ReCaptcha
            services.AddReCaptcha(Configuration.GetSection("ReCaptchaV3"));

            // VFD Configuring Identity properties
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = Configuration.GetSection("PasswordPolicy").GetValue<bool>("RequireDigit",true);
                options.Password.RequireLowercase = Configuration.GetSection("PasswordPolicy").GetValue<bool>("RequireLowercase", true);
                options.Password.RequireNonAlphanumeric = Configuration.GetSection("PasswordPolicy").GetValue<bool>("RequireNonAlphanumeric", true);
                options.Password.RequireUppercase = Configuration.GetSection("PasswordPolicy").GetValue<bool>("RequireUppercase", true);
                options.Password.RequiredLength = Configuration.GetSection("PasswordPolicy").GetValue<int>("RequiredLength", 6);
                options.Password.RequiredUniqueChars = Configuration.GetSection("PasswordPolicy").GetValue<int>("RequiredUniqueChars", 1);

                // Lockout settings.
                /*
                 * Initially last parameter in login form is false - changed to true
                 * var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe,
                 * lockoutOnFailure: true);
                 */
                options.Lockout.DefaultLockoutTimeSpan = 
                    TimeSpan.FromMinutes(Configuration.GetSection("LockoutPolicy").GetValue<int>("DefaultLockoutTimeSpan", 5));
                options.Lockout.MaxFailedAccessAttempts =
                    Configuration.GetSection("LockoutPolicy").GetValue<int>("MaxFailedAccessAttempts", 5);
                options.Lockout.AllowedForNewUsers = 
                    Configuration.GetSection("LockoutPolicy").GetValue<bool>("AllowedForNewUsers", true);

                // User settings.
                options.User.AllowedUserNameCharacters =
                    Configuration.GetSection("UserPolicy").GetValue<string>("AllowedUserNameCharacters", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+");
                options.User.RequireUniqueEmail =
                    Configuration.GetSection("UserPolicy").GetValue<bool>("RequireUniqueEmail", true);

                // Default SignIn settings.
                options.SignIn.RequireConfirmedEmail =
                    Configuration.GetSection("SignInPolicy").GetValue<bool>("RequireConfirmedEmail", true);
                options.SignIn.RequireConfirmedPhoneNumber =
                    Configuration.GetSection("SignInPolicy").GetValue<bool>("RequireConfirmedPhoneNumber", false);
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly =
                    Configuration.GetSection("CoockiePolicy").GetValue<bool>("HttpOnly", true);
                options.ExpireTimeSpan = 
                    TimeSpan.FromMinutes(Configuration.GetSection("CoockiePolicy").GetValue<int>("ExpireTimeSpan", 10));
                options.SlidingExpiration =
                    Configuration.GetSection("CoockiePolicy").GetValue<bool>("SlidingExpiration", true);

                options.LoginPath = Configuration.GetSection("CoockiePolicy").GetValue<string>("LoginPath", "/Identity/Account/Login");
                options.AccessDeniedPath = Configuration.GetSection("CoockiePolicy").GetValue<string>("AccessDeniedPath", "/Identity/Account/AccessDenied");
            });
            // VFD Configuring Identity properties END

            // make antiforgery cookie always secure
            services.AddAntiforgery(
                options =>
                {
                    options.Cookie.Name = "_af";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.HeaderName = "X-XSRF-TOKEN";
                }
            );

            // hanges all data protection tokens timeout period to 3 hours
            services.Configure<DataProtectionTokenProviderOptions>(o =>
                                                                   o.TokenLifespan = TimeSpan.FromHours(3));

            // Adding email service for Identity component
            services.AddTransient<IEmailSender, IdentityEmailSenderTrivial>();
            services.Configure<IdentityMailSenderOptions>(
                                                           Configuration.GetSection("IdentityMailSender")
                                                         );
            // Set the default authentication policy to require users to be authenticated
            // Very important. If you want allow anonimus access to any page just use
            // [AllowAnonymous] before it's PageModel class definition
            services.AddControllers(config =>
            {
                var policy = new AuthorizationPolicyBuilder()
                                 .RequireAuthenticatedUser()
                                 .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
                config.RespectBrowserAcceptHeader = true;
            });
            // Registrate authorization handlers (for injection in partial)
            // if handler dont use EF and all information needed is provided
            // by its parameter object - it can be registyered with AddSingleton AddScoped
            services.AddScoped<IAuthorizationHandler,
                               DealWithAccount_AuthorizationHandler>();
            // SignalR setup
            services.AddSignalR()
                .AddHubOptions<UsersInterconnectHub>(options =>
                {
                    options.EnableDetailedErrors = true;
                    // should be in production becouse of long db and pdf gen operations
                    options.ClientTimeoutInterval = TimeSpan.FromSeconds(Configuration.GetSection("UAStore").GetValue<int>("serverAndClientTimeout", 180));
                    options.KeepAliveInterval = TimeSpan.FromSeconds(Configuration.GetSection("UAStore").GetValue<int>("serverAndClientkeepAliveInterval", 90));
                });
            // To be able obtain current http context in middleware
            services.AddHttpContextAccessor();

            // client to k8s instance of https://thecodingmachine.github.io/gotenberg/
            // https://github.com/ChangemakerStudios/GotenbergSharpApiClient
            services.AddOptions<GotenbergSharpClientOptions>()
                    .Bind(Configuration.GetSection(nameof(GotenbergSharpClient)));
            services.AddGotenbergSharpClient();

            // dotnet add package EnyimMemcachedCore
            // http://memcached.org/
            // https://github.com/cnblogs/EnyimMemcachedCore
            // services.AddEnyimMemcached(Configuration.GetSection("enyimMemcached"));

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "BENEDICTUS",
                    Description = "Unified WEB API for Statistics, ETL and Fird-Party Logging",
                    //TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Valerii Danilov",
                        Email = string.Empty,
                        Url = new Uri("https://www.linkedin.com/in/valerii-danilov-851505124/"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT"),
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description =
        "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/recommended-tags-for-documentation-comments
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

        // https://docs.microsoft.com/en-us/aspnet/core/client-side/bundling-and-minification?view=aspnetcore-5.0
        // https://github.com/Taritsyn/WebMarkupMin
        // https://github.com/Taritsyn/WebMarkupMin/wiki
            // Add WebMarkupMin services.
            services.AddWebMarkupMin(options =>
            {
                options.AllowMinificationInDevelopmentEnvironment = false;
                options.AllowCompressionInDevelopmentEnvironment = false;
                options.DisablePoweredByHttpHeaders = true;
            })
                .AddHtmlMinification(options =>
                {
                    options.ExcludedPages = new List<IUrlMatcher>
                    {
                        new WildcardUrlMatcher("/minifiers/x*ml-minifier"),
                        new ExactUrlMatcher("/contact")
                    };

                    HtmlMinificationSettings settings = options.MinificationSettings;
                    settings.RemoveRedundantAttributes = true;
                    settings.RemoveHttpProtocolFromAttributes = true;
                    settings.RemoveHttpsProtocolFromAttributes = true;
                    settings.MinifyEmbeddedCssCode = true;
                    settings.MinifyEmbeddedJsCode = true;
                    settings.MinifyInlineCssCode = true;
                    settings.MinifyInlineJsCode = true;
                    settings.RemoveHtmlComments = true;
                    settings.RemoveHtmlCommentsFromScriptsAndStyles = true;

                    options.CssMinifierFactory = new KristensenCssMinifierFactory();
                    options.JsMinifierFactory = new CrockfordJsMinifierFactory();

                })
                .AddXhtmlMinification(options =>
                {
                    options.IncludedPages = new List<IUrlMatcher>
                    {
                        new WildcardUrlMatcher("/minifiers/x*ml-minifier"),
                        new ExactUrlMatcher("/contact")
                    };

                    XhtmlMinificationSettings settings = options.MinificationSettings;
                    settings.RemoveRedundantAttributes = true;
                    settings.RemoveHttpProtocolFromAttributes = true;
                    settings.RemoveHttpsProtocolFromAttributes = true;
                    settings.MinifyEmbeddedCssCode = true;
                    settings.MinifyEmbeddedJsCode = true;
                    settings.MinifyInlineCssCode = true;
                    settings.MinifyInlineJsCode = true;
                    settings.RemoveHtmlComments = true;
                    settings.RemoveHtmlCommentsFromScriptsAndStyles = true;

                    options.CssMinifierFactory = new KristensenCssMinifierFactory();
                    options.JsMinifierFactory = new CrockfordJsMinifierFactory();
                })
                .AddXmlMinification(options =>
                {
                    XmlMinificationSettings settings = options.MinificationSettings;
                    settings.CollapseTagsWithoutContent = true;
                })
                .AddHttpCompression(options =>
                {
                    options.CompressorFactories = new List<ICompressorFactory>
                    {
                        new DeflateCompressorFactory(new DeflateCompressionSettings
                        {
                            Level = CompressionLevel.Fastest
                        }),
                        new GZipCompressorFactory(new GZipCompressionSettings
                        {
                            Level = CompressionLevel.Fastest
                        })
                    };
                })
                ;

            // adding controller ip check for WEBAPI
            // https://docs.microsoft.com/en-us/aspnet/core/security/ip-safelist?view=aspnetcore-5.0
            // https://codeburst.io/implementing-and-testing-ip-safelists-in-asp-net-core-dbd9e6f4b696
            services.AddScoped<ClientIpCheckActionFilter>();
            services.AddSingleton(Configuration.GetSection("IpSafeList").Get<IpSafeList>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                              ILoggerFactory loggerFactory)
        {
            GlobalParameters.setLoggerFactory(loggerFactory);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // let's allow unsecure connections, force secure by kubernetes service definition
            // app.UseHttpsRedirection();
            // allow to know real ip if use revers proxy server
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseWebOptimizer();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // block executing in a frame
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                await next();
            });

            // app.UseEnyimMemcached();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BENEDICTUS v2");
                c.RoutePrefix = "BNDv2"; // string.Empty; // to put it into webroot
            });

            app.UseWebMarkupMin();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<UsersInterconnectHub>("/UsersInterconnectHub"); // SignalR setup
                endpoints.MapControllers(); // to use MVC with Endpoints !!!!!
            });
        }
    }
}
