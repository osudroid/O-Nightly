namespace OsuDroid.Class; 

public class LogWrapper(LamLogger.LamLog logger): OsuDroidAttachment.Interface.ILogger {
    public LamLogger.LamLog Logger { get; } = logger;
    
    public ValueTask DisposeAsync() {
        Logger.Dispose();
        return default;
    }

    public async ValueTask CommitAsync() {
        await Logger.FlushToDbAsync();
    }
}