using OsuDroid.Class;
using OsuDroid.Class.Dto;
using OsuDroid.View;
using OsuDroidAttachment.Class;
using OsuDroidAttachment.DbBuilder;
using OsuDroidAttachment.Interface;
using OsuDroidLib.Dto;
using OsuDroidLib.Manager;
using ReturnType = OsuDroidAttachment.Class.OptionHandlerOutput<OsuDroid.View.ViewIsAdmin>;

namespace OsuDroid.Handler; 

public class IsAdminHandler: IHandler<NpgsqlCreates.DbWrapper, LogWrapper, ControllerWrapper,
      OptionHandlerOutput<ViewIsAdmin>>  {
      public async ValueTask<Result<OptionHandlerOutput<ViewIsAdmin>, string>> Handel(
            NpgsqlCreates.DbWrapper dbWrapper, 
            LogWrapper logger, 
            ControllerWrapper request) {

            var db = dbWrapper.Db;
            var cookieAndUserId = request.Controller.GetCookieAndUserId(db);
            if (cookieAndUserId.IsNotSet()) { 
                  return Result<ReturnType, string>.Ok(OptionHandlerOutput<ViewIsAdmin>.With(new ViewIsAdmin() { IsAdmin = false }));
            }

            var userId = cookieAndUserId.Unwrap().UserId;
            var resultPrivileges = await UserGroupPrivilegeManager.GetGroupPrivilegesAsync(db, userId);
            if (resultPrivileges == EResult.Err) { 
                  return resultPrivileges.ChangeOkType<ReturnType>();
            }

            var privileges = resultPrivileges.Ok().GroupPrivileges;
            var isAdmin = false;
            foreach (KeyValuePair<Guid,GroupPrivilegeDto> groupPrivilegeDto in privileges) {
                  if (groupPrivilegeDto.Value.Name != "admin") continue;
                  isAdmin = true;
                  break;
            }
            
            return Result<ReturnType, string>.Ok(OptionHandlerOutput<ViewIsAdmin>
                  .With(new ViewIsAdmin { IsAdmin = isAdmin })
            );
      }
}