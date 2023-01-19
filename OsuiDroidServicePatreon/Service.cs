using OsuDroidLib.AdaPatreon;
using OsuDroidLib.Database.Entities;
using Patreon.NET;

namespace OsuDroidServicePatreon;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Response<ServiceState> RunClean(ServiceState state) {
        using var db = DbBuilder.BuildPostSqlAndOpen();

        WriteLine("Start Update Patreon");

        var adapterPatreon = AdapterPatreon
            .Adapt(new PatreonClient(Env.PatreonAccessToken!))
            .SetCaching(true)
            .SetCachingTime(TimeSpan.FromMinutes(4))
            .SetCanpaignId(Env.PatreonCanpaignId!)
            .Use();

        var task = UpdatePatreonStatusLoop(adapterPatreon, db);
        task.Wait();
        var resp = task.Result;
        WriteLine($"Finish Update Patreon (Status; {resp == EResponse.Ok})");

        return resp == EResponse.Ok ? Response<ServiceState>.Ok(state) : Response<ServiceState>.Err;
    }

    private static async Task<Response> UpdatePatreonStatusLoop(IAdapterPatreon adapterPatreon, SavePoco db) {
        var emailsActivePatronEmails = await adapterPatreon.GetOnlyActivePatronEmails();

#if DEBUG
        if (emailsActivePatronEmails == EResponse.Err)
            throw new Exception("emailsActivePatronEmails == EResponse.Err");

#else
        if (emailsActivePatronEmails == EResponse.Err) {
            Console.WriteLine("Get Error emailsActivePatronEmails");
            return Response.Err();
        }

#endif

        BblPatron.SyncPatronMembersByEmails(db, emailsActivePatronEmails.Ok());
        return Response.Ok();
    }
}