using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler; 

public class ActiveUserHandler : IHandler<NpgsqlCreates.DbWrapper,LogWrapper,ControllerWrapper,OptionHandlerOutput<ViewStatisticActiveUser>> {
    private static (DateTime LastUpdate, (long Last1h, long Last1Day, long Register) Value) Buffer = (DateTime.MinValue,
        default);
    
    public async ValueTask<Result<OptionHandlerOutput<ViewStatisticActiveUser>, string>> Handel(
        NpgsqlCreates.DbWrapper dbWrapper, 
        LogWrapper logger, 
        ControllerWrapper request) {

        var db = dbWrapper.Db;



        
        
        if (Buffer.LastUpdate + TimeSpan.FromMinutes(10) > DateTime.UtcNow) {
            return Result<OptionHandlerOutput<ViewStatisticActiveUser>, string>
                .Ok(OptionHandlerOutput<ViewStatisticActiveUser>
                    .With(ViewStatisticActiveUser.FromStatisticActiveUser(new QueryUserInfo.StatisticActiveUser() {
                        RegisterUser = Buffer.Value.Register,
                        ActiveUserLast1Day = Buffer.Value.Last1Day,
                        ActiveUserLast1H = Buffer.Value.Last1h
                    })));
        }

        var result = await QueryUserInfo.GetStatisticActiveUser(db);

        if (result == EResult.Err) {
            return result.ChangeOkType<OptionHandlerOutput<ViewStatisticActiveUser>>();
        }

        var rep = result.Ok().Or(new() { RegisterUser = 0, ActiveUserLast1Day = 0, ActiveUserLast1H = 0 });

        Buffer = (DateTime.UtcNow, (rep.ActiveUserLast1H, rep.ActiveUserLast1Day, rep.RegisterUser));

        return Result<OptionHandlerOutput<ViewStatisticActiveUser>, string>
            .Ok(OptionHandlerOutput<ViewStatisticActiveUser>
                .With(ViewStatisticActiveUser.FromStatisticActiveUser(new QueryUserInfo.StatisticActiveUser() {
                    RegisterUser = Buffer.Value.Register,
                    ActiveUserLast1Day = Buffer.Value.Last1Day,
                    ActiveUserLast1H = Buffer.Value.Last1h
                })));
    }
}