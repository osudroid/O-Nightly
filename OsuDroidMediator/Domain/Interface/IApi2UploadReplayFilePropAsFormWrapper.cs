namespace OsuDroidMediator.Domain.Interface; 

public interface IApi2UploadReplayFilePropAsFormWrapper {
    public StreamReader? File { get; set; }
    public string? Prop { get; set; }
}