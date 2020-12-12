using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;
using Serilog.Events;
using System.IO;


namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
              Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                       .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           config
                               .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                               .AddJsonFile("appsettings.json", true, true)
                               .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                               .AddJsonFile("configuration.json")
                               .AddJsonFile($"configuration.{hostingContext.HostingEnvironment.EnvironmentName}.json")
                               .AddEnvironmentVariables();
                       })
                      .ConfigureServices(services =>    
                      {
                          //add app insights
                          services.AddApplicationInsightsTelemetry();

                          //add Ocelot
                          services.AddOcelot().AddPolly();

                      })
                      .UseSerilog((_, config) =>
                      {
                          config
                           .MinimumLevel.Information()
                           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                           .Enrich.FromLogContext()
                           .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces);

                      })
                     .Configure(app =>
                     {
                         app.UseOcelot().Wait();
                     });
                 });
    }
}
