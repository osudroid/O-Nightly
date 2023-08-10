using System.Collections.Concurrent;
using OsuDroidLib.Interface;
using OsuDroidLib.Lib;

namespace OsuDroidLib.Manager.TokenHandler;

public static class TokenHandlerManger {
    private static Task? _taskClean;
    private static readonly ConcurrentDictionary<ETokenHander, IRefreshAuto> TokenHandlerDic = new();

    public static ITokenHandlerDb GetOrCreateCacheDatabase() {
        _taskClean ??= Task.Factory.StartNew(async () => {
                while (true)
                    try {
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1));
                            foreach (var (key, o) in TokenHandlerDic)
                                o.RefreshAutoAsync();
                        }
                    }
                    catch (Exception) {
                        // ignored
                    }
            }, TaskCreationOptions.LongRunning
        );

        if (TokenHandlerDic.TryGetValue(ETokenHander.User, out var value)) {
            if (value is ITokenHandlerDb db) return db;
            throw new Exception("TokenHandler is not Type of ITokenHandlerDb Exist Type: " + value.GetType());
        }

        var res = new TokenHandlerDatabase(DateTime.UtcNow, TimeSpan.FromHours(1), TimeSpan.FromDays(32));
        TokenHandlerDic[ETokenHander.User] = res;
        return res;
    }
}