using System.Collections.Concurrent;

namespace OsuDroid.Lib.TokenHandler;

public static class TokenHandlerManger {
    private static Task? _taskClean;
    private static readonly ConcurrentDictionary<ETokenHander, IRefreshAuto> TokenHandlerDic = new();

    public static ITokenHandlerDb GetOrCreateCacheDatabase(ETokenHander eTokenHandler) {
        _taskClean ??= Task.Factory.StartNew(async () => {
            while (true) {
                try {
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1));
                        foreach (var (key, o) in TokenHandlerDic) 
                            o.RefreshAuto();

                    }
                }
                catch (Exception) {
                    // ignored
                }
            }
        }, TaskCreationOptions.LongRunning);

        if (TokenHandlerDic.TryGetValue(eTokenHandler, out var value)) {
            if (value is ITokenHandlerDb db) return db;
            throw new Exception("TokenHandler is not Type of ITokenHandlerDb Exist Type: " + value.GetType());
        }

        var res = new TokenHandlerDatabase(DateTime.UtcNow, TimeSpan.FromHours(1), TimeSpan.FromDays(32));
        TokenHandlerDic[eTokenHandler] = res;
        return res;
    }
}