using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CryptradeNotif.Utilities;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace CryptradeNotif.Services {
    public interface ITokenRequester {
        Task<TokenSchema> GetTokenData(string address);
    }

    public class TokenRequester : ITokenRequester {
        private readonly DiscordSocketClient _client;
        private readonly ITokenWatcher _tokenWatcher;
        private readonly IHttpClientFactory _httpFactory;

        private const string BaseUrl = "https://api.dex.guru/v1/tokens/";

        public TokenRequester(IServiceProvider services) {
            _httpFactory = services.GetRequiredService<IHttpClientFactory>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _tokenWatcher = services.GetRequiredService<ITokenWatcher>();
            
            this.Do();
        }

        private async Task Do() {
            while ( true ) {
                await this.Request();
                await Task.Delay(5000);
            }
        }

        public async Task<TokenSchema> GetTokenData(string address) {
            try {
                var client = _httpFactory.CreateClient();
                client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.26.10");
                
                var result = await client.GetAsync($"{BaseUrl}{address}");
                
                if ( !result.IsSuccessStatusCode ) {
                    Console.WriteLine($"Error {(int) result.StatusCode} trying to receive dex.guru data: {result.ReasonPhrase}");
                    Console.WriteLine($"{result.RequestMessage}");
                    Console.WriteLine($"{result.Content.ToString()}");
                    return null;
                }
                
                var jsonObject = JObject.Parse(await result.Content.ReadAsStringAsync());

                return jsonObject.ToObject<TokenSchema>();
            } catch ( Exception e ) {
                await _client.GetGuild(834456582552813588).GetTextChannel(834456582552813591)
                    .SendMessageAsync($"Error: {e}");
            }

            return null;
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