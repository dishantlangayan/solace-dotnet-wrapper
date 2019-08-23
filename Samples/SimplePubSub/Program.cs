using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SolaceDotNetWrapper.Core;

namespace SimplePubSub
{
    public class Program
    {
        private IConfigurationRoot Configuration { get; set; }
        private readonly IServiceCollection serviceCollection;
        private readonly IServiceProvider serviceProvider;
        private BufferBlock<Message> messageQueue = new BufferBlock<Message>();
        private readonly ILogger logger;

        public Program()
        {
            // Check if development env
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            // Configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables();

            // Add user secrets in dev env only
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }

            Configuration = builder.Build();

            // Create service collection and configure our services
            serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            // Generate a provider
            serviceProvider = serviceCollection.BuildServiceProvider();

            // Logging
            logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        }

        public async Task RunAsync()
        {
            // Kick off our actual code
            var solaceConn = serviceProvider.GetService<IConnection>();

            try
            {
                // Register to receive connection events
                BufferBlock<ConnectionEvent> eventQueue = new BufferBlock<ConnectionEvent>();
                var printEvents = new ActionBlock<ConnectionEvent>(evt =>
                {
                    Console.WriteLine("Connection Event: {0} ResponseCode: {1} Info: {2}",
                       evt.State, evt.ResponseCode, evt.Info);
                });
                eventQueue.LinkTo(printEvents);
                solaceConn.RegisterConnectionEvents(eventQueue);

                // Connect
                var connEvent = await solaceConn.ConnectAsync();
                if (connEvent.State == ConnectionState.Opened)
                {
                    // Subscribe to a topic
                    var topic = new Topic("hello/dishant");
                    await solaceConn.SubscribeAsync(topic);

                    // Publish 10 msgs to the same topic
                    var msg = new NonPersistentMessage()
                    {
                        Destination = topic
                    };

                    for (int i = 0; i < 10; i++)
                    {
                        msg.Body = "Hello msg: " + i;
                        await solaceConn.SendAsync(msg);
                    }

                    // Confirm we have received the messages
                    var rxMsgs = ConsumeFromBuffer();
                    await Task.WhenAll(rxMsgs);

                    // Disconnect
                    await solaceConn.DisconnectAsync();
                }
                else
                {
                    logger.LogWarning("Failed to connect to broker - info: {0} responseCode: {1}",
                        connEvent.Info, connEvent.ResponseCode);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, ex.Message);
            }
        }

        private async Task<int> ConsumeFromBuffer()
        {
            int rxMsgCounter = 0;
            while (rxMsgCounter < 10 && await messageQueue.OutputAvailableAsync())
            {
                var msg = await messageQueue.ReceiveAsync();
                rxMsgCounter++;
                logger.LogInformation("Received Msg - Body: {0}", msg.Body);
            }

            return rxMsgCounter;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Configuration
            services.AddSingleton<SolaceOptions>(Configuration.GetSection("Solace").Get<SolaceOptions>());

            // Logging
            services.AddLogging(configure => configure.AddConsole());

            // Buffer to receive message
            /*var printMsg = new ActionBlock<Message>(message =>
            {
                Console.WriteLine("Received Message on topic: {0}", message.Destination.Name);
            });
            messageQueue.LinkTo(printMsg);*/
            services.AddSingleton(messageQueue);

            // Services
            services.AddSingleton<IConnection, SolaceConnection>();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("SimplePubSub - publishing and receiving 10 messages");
            var program = new Program();
            program.RunAsync().Wait();
            Console.WriteLine("SimplePubSub finished");
        }
    }
}
