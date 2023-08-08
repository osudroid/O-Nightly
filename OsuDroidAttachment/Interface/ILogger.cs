namespace OsuDroidAttachment.Interface; 

public interface ILogger: IAsyncDisposable {
    public ValueTask CommitAsync();
}