using LamLogger;
using ILogger = OsuDroidAttachment.Interface.ILogger;

namespace OsuDroid.Class;

public class LogWrapper(LamLog logger) : ILogger {
    public LamLog Logger { get; } = logger;

    public ValueTask DisposeAsync() {
        Logger.Dispose();
        return default;
    }

    public async ValueTask CommitAsync() {
        await Logger.FlushToDbAsync();
    }
}