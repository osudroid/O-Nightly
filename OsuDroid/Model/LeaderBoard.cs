using Npgsql;
using OsuDroid.Utils;
using OsuDroidLib.Database.Entities;
using OsuDroidLib.Query;

namespace OsuDroid.Model;

public static class LeaderBoard {
    public static async Task<Result<List<LeaderBoardUser>, string>> AnyRegionAsync(NpgsqlConnection db, int limit) {
        return (await QueryUserStats.LeaderBoardNoFilter(db, limit > 100 ? 100 : limit < 1 ? 50 : limit))
            .Map(x => x.ToList());
    }

    public static async Task<Result<List<LeaderBoardUser>, string>> FilterRegionAsync(
        NpgsqlConnection db, int limit, CountryInfo.Country country) {
        
        return (await QueryUserStats.LeaderBoardFilterCountry(
            db, limit > 100 ? 100 : limit < 1 ? 50 : limit, country.NameShort.ToUpper()))
            .Map(x => x.ToList());
    }

    public static async Task<Result<Option<LeaderBoardUser>, string>> UserAsync(NpgsqlConnection db, long userId) {
        return await QueryUserStats.LeaderBoardUserRank(db, userId);
    }

    public static async Task<Result<List<LeaderBoardUser>, string>> SearchUserWithRegionAsync(NpgsqlConnection db, 
        long limit, string query, CountryInfo.Country country) {
        
        return (await QueryUserStats.LeaderBoardSearchUser(
            db, limit > 100 ? 100 : limit < 1 ? 50 : limit, query, country.NameShort.ToUpper()))
            .Map(x => x.ToList());
    }

    public static async Task<Result<List<LeaderBoardUser>, string>> SearchUserAsync(
        NpgsqlConnection db, long limit, string query) {
        
        return (await QueryUserStats.LeaderBoardSearchUser(db, limit > 100 ? 100 : limit < 1 ? 50 : limit, query))
            .Map(x => x.ToList());
    }
}