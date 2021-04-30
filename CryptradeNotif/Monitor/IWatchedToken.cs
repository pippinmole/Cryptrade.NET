namespace CryptradeNotif.Services {
    public interface IWatchedToken {
        ulong UserId { get; }
        string Address { get; }
        decimal TargetPrice { get; }
    }
}