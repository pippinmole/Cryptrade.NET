using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptradeNotif.Services;
using CryptradeNotif.Utilities;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace CryptradeNotif.Modules {
    public class TestCommands : ModuleBase {
        
        private readonly ITokenWatcher _tokenWatcher;
        private readonly CommandService _commands;
        private readonly ITokenRequester _tokenRequester;

        public TestCommands(IServiceProvider services) {
            _tokenWatcher = services.GetRequiredService<ITokenWatcher>();
            _commands = services.GetRequiredService<CommandService>();
            _tokenRequester = services.GetRequiredService<ITokenRequester>();
        }

        [Command("watch")]
        [Summary("Watches a token. Usage: watch {address} {priceUsd}")]
        public async Task WatchAsync(
            [Summary("The token address")] string address, decimal priceUsd) {
            // We can also access the channel from the Command Context.

            var token = new WatchedToken(this.Context.Message.Author.Id, address, priceUsd);
            _tokenWatcher.AddToken(token);

            var embed = new EmbedBuilder {
                Title = "Address added to watchlist",
                Description = $"**{address}** has been added to the watchlist"
            }.WithCurrentTimestamp();;

            await this.ReplyAsync(embed: embed.Build());
        }

        [Command("unwatch")]
        [Summary("Unwatches a token. Usage: unwatch {address}")]
        public async Task UnwatchAsync(
            [Summary("The token address")] string address) {

            if ( address == "all" ) {
                _tokenWatcher.RemoveAll(x => x.UserId == Context.Message.Author.Id);

                var embed = new EmbedBuilder {
                    Title = "Address watchlist cleared",
                    Description = "All addresses have been removed from the watchlist."
                }.WithCurrentTimestamp();;

                await this.ReplyAsync(embed: embed.Build());
            } else {
                // We can also access the channel from the Command Context.
                _tokenWatcher.RemoveAll(x => x.Address == address);

                var embed = new EmbedBuilder {
                    Title = "Address removed from watchlist",
                    Description = $"Address **{address}** is no longer being watched."
                }.WithCurrentTimestamp();;

                await this.ReplyAsync(embed: embed.Build());
            }
        }

        [Command("watchlist")]
        [Summary("Lists all currently watched tokens")]
        public async Task ListWatchlistAsync() {
            var watchlist = _tokenWatcher.GetWatchedTokens();
            var embed = new EmbedBuilder() {
                Title = "Watchlist",
                Description = watchlist.Any()
                    ? string.Join("\n", _tokenWatcher.GetWatchedTokens())
                    : "No tokens watched."
            }.WithCurrentTimestamp();;

            await this.ReplyAsync(embed: embed.Build());
        }

        [Command("stats")]
        [Summary("Retrieves the statistics for a specific coin")]
        public async Task TokenStatsAsync([Summary("Coin address")] string address) {
            var token = await _tokenRequester.GetTokenData(address);

            var embed = new EmbedBuilder()
                .WithTitle("Token Data")
                .AddField("Name", token.Name)
                .AddField("Price", $"${token.PriceUsd}")
                .AddField("Liquidity", $"${token.LiquidityUsd}")
                .AddField("Network", token.Network)
                .WithDescription(new TokenAddress(token.Id).AsEmbed());
            
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        [Command("help")]
        [Summary("Displays all available commands")]
        public async Task HelpAsync() {
            var commands = _commands.Commands.ToList();
            var embedBuilder = new EmbedBuilder().WithCurrentTimestamp();;

            foreach ( var command in commands ) {
                // Get the command Summary attribute information
                var embedFieldText = command.Summary ?? "No description available\n";
                embedBuilder.AddField(command.Name, embedFieldText);
            }

            await ReplyAsync("Here's a list of commands and their description: ", false, embedBuilder.Build());
        }
    }
}