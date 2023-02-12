using Patreon.NET;

namespace OsuDroidLib.AdaPatreon; 

public sealed class AdapterPatreon : IAdapterPatreon, IAdapterPatreonBuild, IDisposable {
    private bool _caching;
    private List<Member>? _cachingMember;
    private TimeSpan _cachingTime;
    private string? _canpaignId;
    private DateTime _lastCachingTime;
    private PatreonClient? _patreonClient;

    public async Task<Result<IReadOnlyList<Member>, EAdapterPatreonError>> GetMembers() {
        if (_patreonClient is null)
            return Result<IReadOnlyList<Member>, EAdapterPatreonError>.Err(EAdapterPatreonError.PatreonClientIsNull);
        if (string.IsNullOrEmpty(_canpaignId))
            return Result<IReadOnlyList<Member>, EAdapterPatreonError>.Err(EAdapterPatreonError.IsNullOrEmpty);

        if (GetCachingMember() is not null)
            return Result<IReadOnlyList<Member>, EAdapterPatreonError>.Ok(GetCachingMember() ?? new List<Member>());


        try {
            var res = await _patreonClient.GetCampaignMembers(_canpaignId);
            if (res is null || res.Count == 0)
                return Result<IReadOnlyList<Member>, EAdapterPatreonError>.Ok(ArraySegment<Member>.Empty);

            SetCachingMember(res);
            return Result<IReadOnlyList<Member>, EAdapterPatreonError>.Ok(res);
        }
        catch (Exception) {
            return Result<IReadOnlyList<Member>, EAdapterPatreonError>.Err(EAdapterPatreonError.Undefined);
        }
    }

    public async Task<Result<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyActivePatronMembers() {
        return (await GetMembers())
            .Map<IReadOnlyList<Member>>(o => o
                .Where(x => x.Attributes.PatreonStatus == "active_patron").ToList());
    }

    public async Task<Result<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyInactivePatronMembers() {
        return (await GetMembers())
            .Map<IReadOnlyList<Member>>(x => x
                .Where(x => x.Attributes.PatreonStatus != "active_patron").ToList());
    }

    public async Task<Result<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyActivePatronEmails() {
        return (await GetMembers()).Map<IReadOnlyList<string>>(f =>
            f.Where(x => x.Attributes.PatreonStatus == "active_patron")
                .Select(x => x.Attributes.Email)
                .ToList());
    }

    public async Task<Result<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyInactivePatronEmails() {
        return (await GetMembers())
            .Map<IReadOnlyList<string>>(f => 
                f.Where(x => x.Attributes.PatreonStatus != "active_patron")
            .Select(x => x.Attributes.Email)
            .ToList());
    }

    public async Task<Result<Option<Member>, EAdapterPatreonError>> GetOnlyOneMemberByEmail(string email) {
        
        // TODO Not Fetch All Members
        return (await GetMembers()).Map(x => Option<Member>.NullSplit(x.FirstOrDefault(x => x.Attributes.Email == email)));
    }

    public void Close() {
        if (_patreonClient is null)
            return;
        _patreonClient.Dispose();
    }

    public void Dispose() {
        _patreonClient?.Dispose();
    }

    public IAdapterPatreonBuild SetCanpaignId(string canpaignId) {
        _canpaignId = canpaignId ?? throw new NullReferenceException(nameof(canpaignId));
        return this;
    }

    public IAdapterPatreonBuild SetCaching(bool on) {
        _caching = on;
        return this;
    }

    public IAdapterPatreonBuild SetCachingTime(TimeSpan cachingTime) {
        _cachingTime = cachingTime;
        return this;
    }

    public IAdapterPatreon Use() {
        return this;
    }

    private List<Member>? GetCachingMember() {
        if (_caching == false
            || _cachingMember is null
            || _lastCachingTime + _cachingTime < DateTime.UtcNow)
            return null;
        return _cachingMember;
    }

    private List<Member> SetCachingMember(List<Member> members) {
        _lastCachingTime = DateTime.UtcNow;
        _cachingMember = members;
        return members;
    }

    public static IAdapterPatreonBuild Adapt(PatreonClient patreonClient) {
        var res = new AdapterPatreon();
        res._patreonClient = patreonClient;

        return res;
    }
}