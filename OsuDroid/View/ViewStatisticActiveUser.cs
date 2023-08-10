using OsuDroidLib.Query;

namespace OsuDroid.View;

// ReSharper disable All
public class ViewStatisticActiveUser : IView {
    public long ActiveUserLast1H { get; set; }
    public long ActiveUserLast1Day { get; set; }
    public long RegisterUser { get; set; }

    public static ViewStatisticActiveUser
        FromStatisticActiveUser(QueryUserInfo.StatisticActiveUser statisticActiveUser) {
        return new ViewStatisticActiveUser {
            ActiveUserLast1H = statisticActiveUser.ActiveUserLast1H,
            ActiveUserLast1Day = statisticActiveUser.ActiveUserLast1Day,
            RegisterUser = statisticActiveUser.RegisterUser
        };
    }
}