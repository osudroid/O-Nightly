using OsuDroid.Class;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Query;

namespace OsuDroid.Handler; 

public class GetAllPatreonHandler : IHandler<NpgsqlCreates.DbWrapper,LogWrapper,ControllerWrapper,OptionHandlerOutput<List<ViewUsernameAndId>>> {
    public async ValueTask<Result<OptionHandlerOutput<List<ViewUsernameAndId>>, string>> Handel(NpgsqlCreates.DbWrapper dbWrapper, LogWrapper logger, ControllerWrapper request) {
        var db = dbWrapper.Db;
        
        var result = (await QueryPatron.GetPatronUser(db)).Map(x => x.ToList());
        if (result == EResult.Err) {
            return result.ChangeOkType<OptionHandlerOutput<List<ViewUsernameAndId>>>();
        }

        var res = ApiTypes.ViewExistOrFoundInfo<List<ViewUsernameAndId>>
                .Exist(result.Ok().Select(x =>
                                 new ViewUsernameAndId { Id = x.UserId, Username = x.Username })
                             .ToList());
      
        
        return Result<OptionHandlerOutput<List<ViewUsernameAndId>>, string>
            .Ok(OptionHandlerOutput<List<ViewUsernameAndId>>.With(res.Value!));
    }
}