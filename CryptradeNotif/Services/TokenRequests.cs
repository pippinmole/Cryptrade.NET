using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CryptradeNotif.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace CryptradeNotif.Services {
    public interface ITokenRequester {

        Task<TokenSchema> GetTokenData(string address);

    }

    public class TokenRequester : ITokenRequester {
        private readonly DiscordSocketClient _client;
        private readonly ITokenWatcher _tokenWatcher;

        private readonly HttpClient _httpClient;

        private const string BaseUrl = "https://api.dex.guru/v1/tokens/";

        public TokenRequester(IServiceProvider services) {
            _client = services.GetRequiredService<DiscordSocketClient>();
            _tokenWatcher = services.GetRequiredService<ITokenWatcher>();

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.26.10");
            
            this.Do();
        }

        private async Task Do() {
            while ( true ) {
                await this.Request();
                await Task.Delay(5000);
            }
        }

        public async Task<TokenSchema> GetTokenData(string address) {
            var result = await _httpClient.GetAsync($"{BaseUrl}{address}");
            var jsonObject = JObject.Parse(await result.Content.ReadAsStringAsync());

            if ( !result.IsSuccessStatusCode ) {
                Console.WriteLine($"Error {(int) result.StatusCode} trying to receive dex.guru data");
                return null;
            }

            return jsonObject.ToObject<TokenSchema>();
        }
        
        private async Task Request() {
            foreach ( var token in this._tokenWatcher.GetWatchedTokens() ) {
                var tokenData = await this.GetTokenData(token.Address);
                var watchedToken = this._tokenWatcher.GetWatchedToken(token.Address);

                if ( watchedToken == null || tokenData == null ) 
                    continue;

                if ( tokenData.PriceUsd > watchedToken.TargetPrice ) {
                    var embed = new EmbedBuilder() {
                        Title = $"{tokenData.Name} Price Alert",
                        Description =
                            $"{new TokenAddress(token.Address).AsEmbed(tokenData.Name)} has hit a price of ${tokenData.PriceUsd}.",
                    };

                    await _client.GetGuild(834456582552813588).GetTextChannel(834456582552813591)
                        .SendMessageAsync($"<@{token.UserId}>", embed: embed.Build());

                    _tokenWatcher.RemoveAll(x => x.Address == token.Address);
                }
            }
        }
    }
}