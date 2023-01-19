using Patreon.NET;

namespace OsuDroidLib.AdaPatreon; 

public interface IAdapterPatreon : IDisposable {
    public void Close();
    public Task<Response<IReadOnlyList<Member>, EAdapterPatreonError>> GetMembers();
    public Task<Response<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyActivePatronMembers();
    public Task<Response<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyInactivePatronMembers();
    public Task<Response<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyActivePatronEmails();
    public Task<Response<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyInactivePatronEmails();
    public Task<Response<Member?, EAdapterPatreonError>> GetOnlyOneMemberByEmail(string email);
}