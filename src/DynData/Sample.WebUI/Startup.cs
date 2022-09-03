using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Sample.WebUI.Configuration;
using Sample.DataAccess;
using a2n.DynData;
using Newtonsoft.Json;
using Sample.WebUI.Data;
using Microsoft.AspNetCore.Identity;

namespace Sample.WebUI
{
    public class Startup
    {
        public IWebHostEnvironment Env { get; }
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            this.Env = env;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = new AppSettings();
            Configuration.Bind("AppSettings", settings);
            services.AddSingleton<AppSettings>(settings);
            AdventureWorksContext.Open(settings.DBConnectionSetting, db =>
            {
                Console.WriteLine("Checking Database...");
                if (db.Database.EnsureCreated())
                {
                    Console.WriteLine(@"Created new database: 
Provider        : {0}
Database Name   : {1}", settings.DBConnectionSetting.Provider.ToString(), db.Database.GetDbConnection().Database);
                }
                else
                {
                    Console.WriteLine(@"Use existing database: 
Provider        : {0}
Database Name   : {1}", settings.DBConnectionSetting.Provider.ToString(), db.Database.GetDbConnection().Database);
                }
            });
            services.AddDbContext<AdventureWorksContext>(o =>
            {
                // without custom event handler
                //o.UseDynData(settings.DBConnectionSetting);
                
                // with custom event handler
                o.UseDynData(settings.DBConnectionSetting, new EventHandlers.AdventureWorksContextHandler());
            });
            #region Compression
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;
            });
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();

                options.MimeTypes = new[]
                {
                    "text/css",
                    "application/javascript",
                    "application/json",
                    "text/json",
                    "application/xml",
                    "text/xml",
                    "text/plain",
                    "image/svg+xml",
                    "application/x-font-ttf"
                };
            });
            #endregion

            #region Cache, Session, Cookie
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(3);
                options.Cookie.Name = ".Sample.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            #endregion

            #region Default Auth
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5;
                options.Password.RequiredUniqueChars = 0;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });
            
            services.AddAuthentication();
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });
            #endregion
            
            // Cache 200 (OK) server responses; any other responses, including error pages, are ignored.
            services.AddResponseCaching();

            var pages = services.AddRazorPages(c =>
            {
                c.RootDirectory = "/Pages";
            });

            // Important!!!
            //pages.AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.PropertyNamingPolicy = null; // remove auto camelcase
            //});
            pages.AddNewtonsoftJson(x =>
            {
                x.UseMemberCasing();
            });

            // enable DynData API
            //services.AddDynDataApi<AdventureWorksContext>("tableOnly"); // without query template ==> api path /dyndata/tableOnly/...
            
            
            services.AddDynDataApi<AdventureWorksContext, AdvWorkQueryTemplate>("adv"); // with query template ==> api path /dyndata/adv/...

            //services.AddDynDataApi<Security.APIAuth, AdventureWorksContext, AdvWorkQueryTemplate>("adv"); // with auth and query template ==> api path /dyndata/adv/...
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppSettings settings, IServiceScopeFactory factory)
        {
            // required for linux environment
            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(x =>
            {
                x.AllowAnyOrigin();
                x.AllowAnyMethod();
                x.AllowAnyHeader();
            });
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            // User response compression
            app.UseResponseCompression();

            // Use HTTPS.
            app.UseHttpsRedirection();

            // Use response compression.
            app.UseResponseCompression();


            //register Auth api required if security enabled
            //app.RegisterDynDataServiceAPIAuth<Security.APIAuth, AdventureWorksContext, AdvWorkQueryTemplate>("adv");
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            using (var scope = factory.CreateScope())
            {
                ApplicationDbContext db = (ApplicationDbContext) scope.ServiceProvider.GetService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            }
        }

    }
}
