using Patreon.NET;

namespace OsuDroidLib.AdaPatreon; 

public interface IAdapterPatreon : IDisposable {
    public void Close();
    public Task<Result<IReadOnlyList<Member>, EAdapterPatreonError>> GetMembers();
    public Task<Result<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyActivePatronMembers();
    public Task<Result<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyInactivePatronMembers();
    public Task<Result<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyActivePatronEmails();
    public Task<Result<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyInactivePatronEmails();
    public Task<Result<Option<Member>, EAdapterPatreonError>> GetOnlyOneMemberByEmail(string email);
}