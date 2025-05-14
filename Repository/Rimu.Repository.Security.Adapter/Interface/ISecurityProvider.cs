namespace Rimu.Repository.Security.Adapter.Interface;

public interface ISecurityProvider {
    public ISecurity Security { get; }
    public ISecurityPhp SecurityPhp { get; }
}