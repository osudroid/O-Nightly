using System.Collections.Concurrent;

namespace OsuDroid.Lib.TokenHandler;

public static class TokenHandlerManger {
    private static Task? TaskClean;
    private static readonly ConcurrentDictionary<ETokenHander, IRefreshAuto> TokenHandlerDic = new();

    public static ITokenHandlerDb GetOrCreateCacheDatabase(ETokenHander eTokenHandler) {
        TaskClean ??= Task.Run(() => {
            try {
                while (true) {
                    Task.Delay(TimeSpan.FromMinutes(1));
                    foreach (var (key, o) in TokenHandlerDic) o.RefreshAuto();
                }
            }
            catch (Exception) {
                // ignored
            }
        });

        if (TokenHandlerDic.TryGetValue(eTokenHandler, out var value)) {
            if (value is ITokenHandlerDb db) return db;
            throw new Exception("TokenHandler is not Type of ITokenHandlerDb Exist Type: " + value.GetType());
        }

        var res = new TokenHandlerDatabase(DateTime.UtcNow, TimeSpan.FromHours(1), TimeSpan.FromDays(32));
        TokenHandlerDic[eTokenHandler] = res;
        return res;
    }
}