using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;

namespace OsuDroid.Model;

public static class LeaderBoard {
    public static Result<List<LeaderBoardUser>, string> AnyRegion(int limit) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardNoFilter(db, limit > 100 ? 100 : limit < 1 ? 50 : limit);
    }

    public static Result<List<LeaderBoardUser>, string> FilterRegion(int limit, CountryInfo.Country country) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardFilterCountry(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, country);
    }

    public static Result<Option<LeaderBoardUser>, string> User(long userId) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardUserRank(db, userId);
    }

    public static Result<List<LeaderBoardUser>, string> SearchUserWithRegion(long limit, string query,
        CountryInfo.Country country) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardSearchUser(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, query, country);
    }

    public static Result<List<LeaderBoardUser>, string> SearchUser(long limit, string query) {
        using var db = DbBuilder.BuildPostSqlAndOpen();
        return SqlFunc.LeaderBoardSearchUser(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, query);
    }
}