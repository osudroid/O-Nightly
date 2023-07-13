using Npgsql;
using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class ModelStatistic {
    private static (DateTime LastUpdate, (long Last1h, long Last1Day, long Register) Value) Buffer = (DateTime.MinValue,
        default);

    public static async Task<Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>, string>>
        ActiveUserAsync(NpgsqlConnection db) {
        if (Buffer.LastUpdate + TimeSpan.FromMinutes(10) > DateTime.UtcNow)
            return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>, string>
                .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>
                    .Ok(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>.Exist(
                        ViewStatisticActiveUser.FromStatisticActiveUser(new QueryUserInfo.StatisticActiveUser() {
                            RegisterUser = Buffer.Value.Register,
                            ActiveUserLast1Day = Buffer.Value.Last1Day,
                            ActiveUserLast1H = Buffer.Value.Last1h
                        }))));

        var result = await QueryUserInfo.GetStatisticActiveUser(db);

        if (result == EResult.Err)
            return result.ChangeOkType<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>>();

        var rep = result.Ok().Or(new() { RegisterUser = 0, ActiveUserLast1Day = 0, ActiveUserLast1H = 0 });

        Buffer = (DateTime.UtcNow, (rep.ActiveUserLast1H, rep.ActiveUserLast1Day, rep.RegisterUser));

        return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>, string>
            .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>>
                .Ok(ApiTypes.ViewExistOrFoundInfo<ViewStatisticActiveUser>.Exist(
                    ViewStatisticActiveUser.FromStatisticActiveUser(new QueryUserInfo.StatisticActiveUser() {
                        RegisterUser = Buffer.Value.Register,
                        ActiveUserLast1Day = Buffer.Value.Last1Day,
                        ActiveUserLast1H = Buffer.Value.Last1h
                    }))));
    }

    public static async Task<Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>>, string>>
        GetActivePatreonAsync(NpgsqlConnection db) {
        var result = (await QueryPatron.GetPatronUser(db)).Map(x => x.ToList());
        if (result == EResult.Err)
            return result.ChangeOkType<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>>>();

        return Result<ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>>, string>
            .Ok(ModelResult<ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>>.Ok(
                ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>
                        .Exist(result.Ok().Select(x =>
                                         new ViewUsernameAndId { Id = x.UserId, Username = x.Username })
                                     .ToList())));
    }
}