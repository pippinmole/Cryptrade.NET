using Newtonsoft.Json;

namespace CryptradeNotif.Services {
    /// <summary>
    /// The schema used for dex.guru's API. Not all fields are here, but you can just add them however you like.
    /// </summary>
    public class TokenSchema {
        [JsonProperty("AMM")] public string Amm;
        [JsonProperty("blockNumber")] public int BlockNumber;
        [JsonProperty("decimals")] public int Decimals;
        [JsonProperty("description")] public string Description;
        [JsonProperty("id")] public string Id;
        [JsonProperty("liquidityChange24h")] public float LiquidityChange24H;
        [JsonProperty("liquidityETH")] public float LiquidityEth;
        [JsonProperty("liquidityUSD")] public float LiquidityUsd;
        [JsonProperty("name")] public string Name;
        [JsonProperty("network")] public string Network;
        [JsonProperty("priceUSD")] public decimal PriceUsd;
    }
}