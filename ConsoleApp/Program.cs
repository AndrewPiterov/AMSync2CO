using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Models;
using EnsureThat;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace ConsoleApp
{
    /*
     * Сделать console.app, который на вход принимает файл
     * со списком BillingSystemSubscriptionReference и планов AMXP/AMQP. Загружает подписку из 2CO и апдейтит план в 2CO
     */
    internal static class Program
    {
        private static IConfigurationRoot _configuration;
         private static IServiceProvider _serviceProvider;
         private static ILogger _logger;
         
        private static async Task Main()
        {
            if (!Config())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Could not configure micro-service!");

                Console.ResetColor();
                Console.WriteLine("Press <Endter> to exit...");
                Console.ReadLine();
                return;
            }
            
            Console.WriteLine("Choose file: ");
            var filePath = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("File path is empty!");
            }

            if (!File.Exists(filePath))
            { 
                throw new Exception($"File '{filePath}' not found!");
            }
            
            await ProcessFile(filePath);

            Console.WriteLine("Success sync");
            Console.ReadKey();
        }

        private static async Task ProcessFile(string filePath)
        {
            var service = _serviceProvider.GetService<AvangateApi>();
            
            // Open file and read
            var lines = File.ReadLines(filePath!);
            foreach (var line in lines)
            {
                await ProcessLine(line, service);
            }
        }

        private static async Task ProcessLine(string line, AvangateApi service)
        {
            EnsureArg.IsNotNullOrEmpty(line, nameof(line));
            EnsureArg.IsNotNull(service, nameof(service));

            var arr = line.Split(",");

            if (arr.Length != 2)
            {
                throw new Exception($"Line '{line}' is invalid");
            }
            
            var subscriptionRef = arr[0];
            var productId = arr[1];

            var productName = "";
            var orgCount = -1;
            
            throw new NotImplementedException();

            // TODO: FetchSubscriptionAsync
            var subscription = await service.FetchSubscriptionAsync(subscriptionRef, CancellationToken.None);
            
            
            // TODO: UpdateSubscriptionAsync
            await service.UpdateSubscriptionAsync(subscription, orgCount, productName, productId, CancellationToken.None);
        }

        private static bool Config()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

            Console.WriteLine("Environment: {0}", environment);

            var services = new ServiceCollection();

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);

            if (environment == "Development")
            {
                builder
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory,
                            string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar),
                            $"appsettings.{environment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder
                    .AddJsonFile($"appsettings.{environment}.json", optional: true);
            }

            _configuration = builder.Build();

            // Logging
            var logfile = _configuration["Logging:FilePath"]
                          ?? $"{AppDomain.CurrentDomain.BaseDirectory}_logs\\{{Date}}.txt";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(logfile)
                .CreateLogger();

            _logger = Log.Logger;

            // Add services
            // services.AddOptions();

            services.AddSingleton<IHashUtils, HashUtils>();

            services.AddSingleton<AvangateSettings>(x => _configuration.GetSection("AvangateSettings").Get<AvangateSettings>());
            services.AddSingleton<AvangateApi>();

            _serviceProvider = services.BuildServiceProvider();

            return true;
        }
    }
}