namespace OsuDroidLib.AdaPatreon;

public interface IAdapterPatreonBuild : IDisposable {
    public void Close();
    public IAdapterPatreon Use();
    public IAdapterPatreonBuild SetCanpaignId(string canpaignId);
    public IAdapterPatreonBuild SetCaching(bool on);
    public IAdapterPatreonBuild SetCachingTime(TimeSpan cachingTime);
}