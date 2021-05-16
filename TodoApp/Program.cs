using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Logging;
using TodoApp.Data;

namespace TodoApp
{
    class Program
    {
        public static Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            try
            {
                var services = ConfigureServices();

                using var serviceProvider = services.BuildServiceProvider();

                // Get the controller service and run it.
                var controller = serviceProvider.GetService<TodoController>();

                // Run the application.
                return controller.Run();
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                Console.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine("Oops, the program terminating due to unexpected error.");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }

            return Task.CompletedTask;
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var config = LoadConfiguration();
            var configSection = config.GetSection("AppConfiguration");

            // Add configuration.
            services.Configure<AppConfiguration>(configSection);

            services.AddSingleton<TodoController>();
            services.AddSingleton<ITodoManager, TodoManager>();

            var logLevel = (Microsoft.Extensions.Logging.LogLevel)config.GetValue(typeof(Microsoft.Extensions.Logging.LogLevel), 
                "Logging:LogLevel:Default", Microsoft.Extensions.Logging.LogLevel.Trace);

            services.AddLogging(loggingBuilder =>
            {
                 // configure Logging with NLog
                 loggingBuilder.ClearProviders();
                 loggingBuilder.SetMinimumLevel(logLevel);
                 loggingBuilder.AddNLog(config);
            });

            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

    }
}
