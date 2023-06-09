using Npgsql;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class Statistic {
    private static (DateTime LastUpdate, (long Last1h, long Last1Day, long Register) Value) Buffer = (DateTime.MinValue,
        default);

    public static async Task<Result<(long Last1h, long Last1Day, long Register), string>> ActiveUserAsync(NpgsqlConnection db) {
        if (Buffer.LastUpdate + TimeSpan.FromMinutes(10) > DateTime.UtcNow)
            return Result<(long Last1h, long Last1Day, long Register), string>.Ok(Buffer.Value);

        var result = await QueryUserInfo.GetStatisticActiveUser(db);
        
        if (result == EResult.Err)
            return result.ChangeOkType<(long Last1h, long Last1Day, long Register)>();
        
        var rep = result.Ok().Or(new() { RegisterUser = 0, ActiveUserLast1Day = 0, ActiveUserLast1H = 0});
        
        Buffer = (DateTime.UtcNow, (rep.ActiveUserLast1H, rep.ActiveUserLast1Day, rep.RegisterUser));

        return Result<(long Last1h, long Last1Day, long Register), string>.Ok(Buffer.Value);
    }

    public static async Task<Result<List<(string Username, long Id)>, string>> GetActivePatreonAsync(NpgsqlConnection db) {
        return (await QueryPatron.GetPatronUser(db)).Map(x => x.ToList());
    }
}