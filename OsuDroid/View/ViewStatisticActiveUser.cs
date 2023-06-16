using OsuDroidLib.Query;

namespace OsuDroid.View; 

public class ViewStatisticActiveUser {
    public long ActiveUserLast1H { get; set; }
    public long ActiveUserLast1Day { get; set; }
    public long RegisterUser { get; set; }

    public static ViewStatisticActiveUser
        FromStatisticActiveUser(QueryUserInfo.StatisticActiveUser statisticActiveUser) {

        return new() {
            ActiveUserLast1H = statisticActiveUser.ActiveUserLast1H,
            ActiveUserLast1Day = statisticActiveUser.ActiveUserLast1Day,
            RegisterUser = statisticActiveUser.RegisterUser,
        };
    }
}