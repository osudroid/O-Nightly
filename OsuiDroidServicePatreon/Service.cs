using Npgsql;
using OsuDroidLib.AdaPatreon;
using OsuDroidLib.Database.Entities;
using Patreon.NET;
using OsuDroidLib.Query;

namespace OsuDroidServicePatreon;

public static class Service {
    public static ServiceState StateBuilder() {
        return new ServiceState();
    }

    public static Result<ServiceState, string> RunClean(ServiceState state) {

        WriteLine("Start Update Patreon");
        
        var adapterPatreon = AdapterPatreon
            .Adapt(new PatreonClient(Setting.Patreon_AccessToken!.Value))
            .SetCaching(true)
            .SetCachingTime(TimeSpan.FromMinutes(4))
            .SetCanpaignId(Setting.Patreon_CampaignId!.Value.ToString())
            .Use();

        var task = UpdatePatreonStatusLoop(adapterPatreon, DbBuilder.BuildNpgsqlConnection().GetAwaiter().GetResult());
        task.Wait();
        var resp = task.Result;
        WriteLine($"Finish Update Patreon (Status: Work = {resp == EResult.Ok})");
        
        return resp == EResult.Ok 
            ? Result<ServiceState, string>.Ok(state) 
            : Result<ServiceState, string>.Err(resp.Err());
    }

    private static async Task<ResultErr<string>> UpdatePatreonStatusLoop(IAdapterPatreon adapterPatreon, NpgsqlConnection db) {
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

        return await QueryPatron.SyncPatronMembersByEmails(db, emailsActivePatronEmails.Ok());
    }
}