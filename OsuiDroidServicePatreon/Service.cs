using OsuDroidLib.AdaPatreon;
using OsuDroidLib.Database.Entities;
using Patreon.NET;

namespace OsuDroidServicePatreon;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Result<ServiceState, string> RunClean(ServiceState state) {
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
        WriteLine($"Finish Update Patreon (Status: Work = {resp == EResult.Ok})");
        
        return resp == EResult.Ok 
            ? Result<ServiceState, string>.Ok(state) 
            : Result<ServiceState, string>.Err(resp.Err());
    }

    private static async Task<ResultErr<string>> UpdatePatreonStatusLoop(IAdapterPatreon adapterPatreon, SavePoco db) {
        var emailsActivePatronEmails = await adapterPatreon.GetOnlyActivePatronEmails();

#if DEBUG
        if (emailsActivePatronEmails == EResult.Err)
            throw new Exception("emailsActivePatronEmails == EResult.Err");

#else
        if (emailsActivePatronEmails == EResult.Err) {
            Console.WriteLine("Get Error emailsActivePatronEmails");
            return ResultErr<string>.Err("emailsActivePatronEmails == EResult.Err");
        }

#endif

        return BblPatron.SyncPatronMembersByEmails(db, emailsActivePatronEmails.Ok());
    }
}