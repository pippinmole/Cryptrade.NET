namespace CryptradeNotif.Utilities {
    public class TokenAddress {
        
        private readonly string _address;
        private readonly string _urlAddress;
        
        public TokenAddress(string address) {
            _address = address;
            _urlAddress = address.Replace("-bsc", "");
        }

        public string AsBscScan() => $"https://bscscan.com/token/{this._urlAddress}";
        public string AsEmbed() => $"[{this._urlAddress}](https://bscscan.com/token/{this._urlAddress})";
        public string AsEmbed(string text) => $"[{text}](https://bscscan.com/token/{this._urlAddress})";
    }
}