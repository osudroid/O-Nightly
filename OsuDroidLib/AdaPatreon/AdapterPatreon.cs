using Patreon.NET;

namespace OsuDroidLib.AdaPatreon; 

public sealed class AdapterPatreon : IAdapterPatreon, IAdapterPatreonBuild, IDisposable {
    private bool _caching;
    private List<Member>? _cachingMember;
    private TimeSpan _cachingTime;
    private string? _canpaignId;
    private DateTime _lastCachingTime;
    private PatreonClient? _patreonClient;

    public async Task<Response<IReadOnlyList<Member>, EAdapterPatreonError>> GetMembers() {
        if (_patreonClient is null)
            return Response<IReadOnlyList<Member>, EAdapterPatreonError>.Err(EAdapterPatreonError.PatreonClientIsNull);
        if (string.IsNullOrEmpty(_canpaignId))
            return Response<IReadOnlyList<Member>, EAdapterPatreonError>.Err(EAdapterPatreonError.IsNullOrEmpty);

        if (GetCachingMember() is not null)
            return Response<IReadOnlyList<Member>, EAdapterPatreonError>.Ok(GetCachingMember() ?? new List<Member>());


        try {
            var res = await _patreonClient.GetCampaignMembers(_canpaignId);
            if (res is null || res.Count == 0)
                return Response<IReadOnlyList<Member>, EAdapterPatreonError>.Ok(ArraySegment<Member>.Empty);

            SetCachingMember(res);
            return Response<IReadOnlyList<Member>, EAdapterPatreonError>.Ok(res);
        }
        catch (Exception) {
            return Response<IReadOnlyList<Member>, EAdapterPatreonError>.Err(EAdapterPatreonError.Undefined);
        }
    }

    public async Task<Response<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyActivePatronMembers() {
        var members = await GetMembers();
        return (EResponse)members switch {
            EResponse.Ok => Response<IReadOnlyList<Member>, EAdapterPatreonError>
                .Ok(members.Ok().Where(x => x.Attributes.PatreonStatus == "active_patron").ToList()),
            _ => members
        };
    }

    public async Task<Response<IReadOnlyList<Member>, EAdapterPatreonError>> GetOnlyInactivePatronMembers() {
        var members = await GetMembers();
        return (EResponse)members switch {
            EResponse.Ok => Response<IReadOnlyList<Member>, EAdapterPatreonError>
                .Ok(members.Ok().Where(x => x.Attributes.PatreonStatus != "active_patron").ToList()),
            _ => members
        };
    }

    public async Task<Response<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyActivePatronEmails() {
        var members = await GetMembers();
        var res = (EResponse)members switch {
            EResponse.Ok => Response<IReadOnlyList<string>, EAdapterPatreonError>
                .Ok(members
                    .Ok()
                    .Where(x => x.Attributes.PatreonStatus == "active_patron")
                    .Select(x => x.Attributes.Email)
                    .ToList()),
            _ => Response<IReadOnlyList<string>, EAdapterPatreonError>.Err(members.Err())
        };
        return res;
    }

    public async Task<Response<IReadOnlyList<string>, EAdapterPatreonError>> GetOnlyInactivePatronEmails() {
        var members = await GetMembers();
        var res = (EResponse)members switch {
            EResponse.Ok => Response<IReadOnlyList<string>, EAdapterPatreonError>
                .Ok(members
                    .Ok()
                    .Where(x => x.Attributes.PatreonStatus != "active_patron")
                    .Select(x => x.Attributes.Email)
                    .ToList()),
            _ => Response<IReadOnlyList<string>, EAdapterPatreonError>.Err(members.Err())
        };
        return res;
    }

    public async Task<Response<Member?, EAdapterPatreonError>> GetOnlyOneMemberByEmail(string email) {
        // TODO Not Fetch All Members
        var res = await GetMembers();
        return (EResponse)res switch {
            EResponse.Err => Response<Member?, EAdapterPatreonError>.Err(res.Err()),
            EResponse.Ok => Response<Member?, EAdapterPatreonError>.Ok(res.Ok()
                .FirstOrDefault(x => x.Attributes.Email == email)),
            _ => throw new ArgumentOutOfRangeException()
        };
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