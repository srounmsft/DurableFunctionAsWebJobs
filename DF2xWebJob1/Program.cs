using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.IO;

namespace DF2xWebJobsdk3x
{
    public static class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder();

            builder.ConfigureWebJobs(config =>
            {
                config.AddAzureStorageCoreServices();
                config.AddAzureStorage(); //webjobs.extensions.storage needed

                    config.AddTimers();
                config.AddDurableTask(options =>
                {
                        //options.HubName = "MyTaskHub";
                        //options.StorageProvider["ConnectionStringName"] = "AzureWebJobsStorage";

                    });
            });
            builder.ConfigureHostConfiguration(configHost =>
            {
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                    // configHost.AddJsonFile("host.json", optional: true);

                });
            builder.ConfigureAppConfiguration((hostcontext, configapp) =>
            {
                configapp.AddJsonFile("appsettings.json", optional: false);
                    // configapp.AddEnvironmentVariables();

                });

            builder.ConfigureLogging((context, logging) =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConsole();
                logging.AddApplicationInsightsWebJobs(config =>
                   {
                       config.InstrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                   });
            });

            builder.UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                host.Run();
            }

        }
    }
}
