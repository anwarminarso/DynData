
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Sample.WebUI
{
    public class Program
    {
        private static CancellationTokenSource cancelTokenSource = new System.Threading.CancellationTokenSource();
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.RunAsync(cancelTokenSource.Token).GetAwaiter().GetResult();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)

                  .ConfigureLogging(logging =>
                  {
                      logging.ClearProviders();
                      logging.AddConsole();
                      logging.AddDebug();
                  })
                  .ConfigureWebHostDefaults(webBuilder =>
                  {
                      webBuilder.ConfigureKestrel(serverOptions =>
                      {
                      });
                      webBuilder.UseStartup<Startup>();
                  });
            return host;
        }
        public static void Shutdown()
        {
            cancelTokenSource.Cancel();
        }
    }
}
