namespace CryptradeNotif.Services {
    public class WatchedToken : IWatchedToken {
        public WatchedToken(ulong userId, string address, decimal price) {
            UserId = userId;
            Address = address;
            TargetPrice = price;
        }
        public ulong UserId { get; }
        public string Address { get; }
        public decimal TargetPrice { get; }
    }
}