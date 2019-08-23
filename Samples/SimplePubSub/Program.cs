using System;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolaceDotNetWrapper.Core;

namespace SimplePubSub
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static BufferBlock<Message> messageQueue = new BufferBlock<Message>();

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            // Solace options
            services
                .Configure<SolaceOptions>(Configuration.GetSection("Solace"))
                .AddOptions();

            // Logging
            services.AddLogging(configure => configure.AddConsole());

            // Buffer to receive message
            // TODO: remove and replace with ActionBlock that increments stats
            var printMsg = new ActionBlock<Message>(message =>
            {
                Console.WriteLine("Received Message on topic: {0}",
                   message.Destination.Name);
            });
            messageQueue.LinkTo(printMsg);
            services.AddSingleton(messageQueue);

            // Services
            services.AddSingleton<IConnection, SolaceConnection>();

            return services;
        }


        static void Main(string[] args)
        {
            // Check if development env
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            // Configuration
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            // Add user secrets in dev env only
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            Configuration = builder.Build();

            // Create service collection and configure our services
            var services = ConfigureServices();
            // Generate a provider
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            // Kick off our actual code
            //var solaceConn = serviceProvider.GetService<IConnection>();

            //try
            //{
                //var connEvent = await solaceConn.ConnectAsync();
            //}
            //catch(Exception ex)
            //{
            //    logger.Log(LogLevel.Error, ex, ex.Message);
            //}
        }
    }
}
