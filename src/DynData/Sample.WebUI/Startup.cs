using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Sample.WebUI.Configuration;
using Sample.DataAccess;
using a2n.DynData;

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


            // Cache 200 (OK) server responses; any other responses, including error pages, are ignored.
            services.AddResponseCaching();

            var pages = services.AddRazorPages(c =>
            {
                c.RootDirectory = "/Pages";
            });


            // Important!!!
            pages.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // remove auto camelcase
            });


            // enable DynData API
            services.AddDynDataApi<AdventureWorksContext, AdvWorkQueryTemplate>("adv");
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppSettings settings)
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
            // User response compression
            app.UseResponseCompression();

            // Use HTTPS.
            app.UseHttpsRedirection();

            // Use response compression.
            app.UseResponseCompression();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }

    }
}
