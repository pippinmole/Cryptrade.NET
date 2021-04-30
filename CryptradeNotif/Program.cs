using System;
using System.Threading.Tasks;
using CryptradeNotif.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptradeNotif {
    public class Program {

        private readonly IConfiguration _config;
        private DiscordSocketClient _client;

        private static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        
        public Program() {
            // create the configuration
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public async Task MainAsync() {
            // call ConfigureServices to create the ServiceCollection/Provider for passing around the services
            await using var services = ConfigureServices();
            
            // get the client and assign to client 
            // you get the services via GetRequiredService<T>
            var client = services.GetRequiredService<DiscordSocketClient>();
            _client = client;

            // setup logging and the ready event
            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            // this is where we get the Token value from the configuration file, and start the bot
            await client.LoginAsync(TokenType.Bot, _config["Token"]);
            await client.StartAsync();

            // we get the CommandHandler class here and call the InitializeAsync method to start things up for the CommandHandler service
            await services.GetRequiredService<CommandHandler>().InitializeAsync();
            services.GetRequiredService<ITokenRequester>();

            await Task.Delay(-1);
        }

        private ServiceProvider ConfigureServices() {
            // this returns a ServiceProvider that is used later to call for those services
            // we can add types we have access to here, hence adding the new using statement:
            // using csharpi.Services;
            // the config we build is also added, which comes in handy for setting the command prefix!
            return new ServiceCollection()
                .AddSingleton(_config)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<ITokenRequester, TokenRequester>()
                .AddSingleton<ITokenWatcher, TokenWatcher>()
                .BuildServiceProvider();
        }

        private Task ReadyAsync() {
            return Task.CompletedTask;
        }

        private static Task LogAsync(LogMessage log) {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
