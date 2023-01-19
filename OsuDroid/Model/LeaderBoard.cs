using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class LeaderBoard {
    public static Response<List<LeaderBoardUser>> AnyRegion(int limit) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardNoFilter(db, limit > 100 ? 100 : limit < 1 ? 50 : limit);
    }

    public static Response<List<LeaderBoardUser>> FilterRegion(int limit, CountryInfo.Country country) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardFilterCountry(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, country);
    }

    public static Response<LeaderBoardUser> User(long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardUserRank(db, userId);
    }

    public static Response<List<LeaderBoardUser>> SearchUserWithRegion(long limit, string query,
        CountryInfo.Country country) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardSearchUser(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, query, country);
    }

    public static Response<List<LeaderBoardUser>> SearchUser(long limit, string query) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardSearchUser(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, query);
    }
}