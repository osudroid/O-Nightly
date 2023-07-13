namespace OsuDroidMediator.Domain.Interface; 

public interface IApi2UploadReplayFile {
    public string? MapHash { get; set; }
    public long ReplayId { get; set; } 
}