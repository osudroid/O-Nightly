using Dapper;

namespace OsuDroid.Lib.DbTransfer; 

internal static class CalcUserScoreHandler {
    public static async Task Run() {
        List<Entities.UserStats> userStatsList;
        {
            List<Entities.PlayScore> playScoreList = await GetAllPlayScore();
            GC.Collect();
            
            List<Entities.UserInfo> userInfoList = await GetAllUserInfoIds();
            userStatsList = await CalcUserStats(playScoreList, userInfoList);
            GC.Collect();
        }
        
        await InsertUserStats(userStatsList);
    }

    private static async Task<List<Entities.PlayScore>> GetAllPlayScore() {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        return (await db.QueryAsync<Entities.PlayScore>("SELECT UserId FROM public.PlayScore")).ToList();
    }
    
    private static async Task<List<Entities.UserInfo>> GetAllUserInfoIds() {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        return (await db.QueryAsync<Entities.UserInfo>("SELECT UserId FROM public.UserInfo")).ToList();
    }

    private static async Task<List<Entities.UserStats>> CalcUserStats(
        List<Entities.PlayScore> playScoreList,List<Entities.UserInfo> userInfoList) {
        
        throw new NotImplementedException(nameof(InsertUserStats));
    }

    private static async Task InsertUserStats(List<Entities.UserStats> userStatsList) {
        throw new NotImplementedException(nameof(InsertUserStats));
    }
}