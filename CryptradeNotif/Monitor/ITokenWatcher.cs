using System;
using System.Collections.Generic;

namespace CryptradeNotif.Services {
    public interface ITokenWatcher {
        void AddToken(IWatchedToken token);
        void RemoveAll(Predicate<IWatchedToken> condition);
        IWatchedToken GetWatchedToken(string address);
        IEnumerable<IWatchedToken> GetWatchedTokens();
    }
}