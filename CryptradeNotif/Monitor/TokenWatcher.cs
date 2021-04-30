using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptradeNotif.Services {
    public class TokenWatcher : ITokenWatcher {

        private readonly List<IWatchedToken> _tokens = new();

        public void AddToken(IWatchedToken token) {
            if ( this._tokens.Exists(x => x.Address == token.Address) ) {
                this._tokens.RemoveAll(x => x.Address == token.Address);
            }

            this._tokens.Add(token);
        }

        public void RemoveAll(Predicate<IWatchedToken> condition) {
            this._tokens.RemoveAll(condition);
        }

        public IWatchedToken GetWatchedToken(string address) {
            return this._tokens.FirstOrDefault(x => x.Address == address);
        }

        public IEnumerable<IWatchedToken> GetWatchedTokens() {
            return this._tokens;
        }
    }
}