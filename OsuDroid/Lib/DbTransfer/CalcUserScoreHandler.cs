using System.Collections.Concurrent;
using Dapper;
using OsuDroidLib.Dto;

namespace OsuDroid.Lib.DbTransfer;

internal static class CalcUserScoreHandler {
    public static async Task Run() {
        WriteLine("Run CalcUserScoreHandler");

        List<Entities.UserStats> userStatsList;
        {
            WriteLine("Run CalcUserScoreHandler GetAllPlayScores");
            var playScoreList = await GetAllPlayScoreAsDto();
            GC.Collect();

            WriteLine("Run CalcUserScoreHandler GetAllUserIds");
            var userInfoList = await GetAllUserInfoIds();

            WriteLine("Run CalcUserScoreHandler CalcStats");
            userStatsList = CalcUserStats(playScoreList, userInfoList);
        }

        WriteLine("Run CalcUserScoreHandler Insert");
        GC.Collect();
        await InsertUserStats(userStatsList);
    }

    private static async Task<List<PlayScoreDto>> GetAllPlayScoreAsDto() {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        var list = (await db.QueryAsync<Entities.PlayScore>("SELECT * FROM public.PlayScore")).ToList();
        var listResult = new List<PlayScoreDto>(list.Count);
        foreach (var playScore in list) {
            var option = PlayScoreDto.ToPlayScoreDto(playScore);
            if (option.IsNotSet())
                throw new Exception($"Convert to Dto not work id: {playScore.PlayScoreId}");
            listResult.Add(option.Unwrap());
        }

        return listResult;
    }

    private static async Task<List<Entities.UserInfo>> GetAllUserInfoIds() {
        await using var db = await DbBuilder.BuildNpgsqlConnection();
        return (await db.QueryAsync<Entities.UserInfo>("SELECT UserId FROM public.UserInfo")).ToList();
    }

    private static List<Entities.UserStats> CalcUserStats(
        List<PlayScoreDto> playScoreList,
        List<Entities.UserInfo> userInfoList) {
        Dictionary<long, ConcurrentBag<PlayScoreDto>> dic = new(userInfoList.Count);

        foreach (var userInfo in userInfoList) dic.Add(userInfo.UserId, new ConcurrentBag<PlayScoreDto>());

        WriteLine("Fill Dictionary With PlayScores");
        Parallel.ForEach(
            playScoreList,
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            v => {
                if (!dic.TryGetValue(v.UserId, out var bag))
                    return;

                bag.Add(v);
            }
        );
        GC.Collect();

        var userStatsList = new List<Entities.UserStats>(dic.Count);

        WriteLine("Calc");
        Parallel.ForEach(
            dic.ToList(),
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            pair => {
                WriteLine("Calc User: " + pair.Key);
                userStatsList.Add(CreateUserStats(pair.Key, pair.Value.ToArray()));
            }
        );

        return userStatsList;
    }

    private static async Task InsertUserStats(List<Entities.UserStats> userStatsList) {
        await using var db = await DbBuilder.BuildNpgsqlConnection();

        await db.ExecuteAsync(@"
INSERT INTO UserStats (userid, overallplaycount, overallscore, overallaccuracy, overallcombo, overallxss, overallxs, overallss, overalls, overalla, overallb, overallc, overalld, overallperfect, overallhits, overall300, overall100, overall50, overallgeki, overallkatu, overallmiss)
VALUES (
        @UserId, 
        @OverallPlaycount, 
        @OverallScore, 
        @OverallAccuracy, 
        @OverallCombo, 
        @OverallXss, 
        @OverallXs, 
        @OverallSs, 
        @OverallS, 
        @OverallA, 
        @OverallB, 
        @OverallC, 
        @OverallD, 
        @OverallPerfect, 
        @OverallHits, 
        @Overall300, 
        @Overall100, 
        @Overall50, 
        @OverallGeki, 
        @OverallKatu, 
        @OverallMiss
);
", userStatsList
        );
    }

    private static Entities.UserStats CreateUserStats(long userId, PlayScoreDto[] playScoreDtoList) {
        var bblUserStats = new Entities.UserStats {
            UserId = userId,
            OverallPlaycount = 0,
            OverallScore = 0,
            OverallAccuracy = 0,
            OverallCombo = 0,
            OverallXss = 0,
            OverallSs = 0,
            OverallXs = 0,
            OverallS = 0,
            OverallA = 0,
            OverallB = 0,
            OverallC = 0,
            OverallD = 0,
            OverallPerfect = 0,
            OverallHits = 0,
            Overall300 = 0,
            Overall100 = 0,
            Overall50 = 0,
            OverallGeki = 0,
            OverallKatu = 0,
            OverallMiss = 0
        };

        if (playScoreDtoList.Length == 0) {
            WriteLine($"UserId: {userId} Has Not Plays");
            return bblUserStats;
        }


        var dictionary = new Dictionary<string, PlayScoreDto>(playScoreDtoList.Length);

        foreach (var playScore in playScoreDtoList) {
            if (string.IsNullOrEmpty(playScore.Hash)) continue;

            if (dictionary.TryGetValue(playScore.Hash, out var inDic) == false) {
                dictionary[playScore.Hash] = playScore;
                continue;
            }

            if (inDic.Score > playScore.Score)
                continue;

            dictionary[playScore.Hash] = playScore;
        }

        bblUserStats.OverallPlaycount = dictionary.Count;
        foreach (var (key, dto) in dictionary) {
            bblUserStats.OverallScore += dto.Score;
            bblUserStats.OverallAccuracy += dto.Accuracy;
            bblUserStats.OverallCombo += dto.Combo;
            bblUserStats.OverallXss += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.XSS);
            bblUserStats.OverallSs += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.SS);
            bblUserStats.OverallXs += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.XS);
            bblUserStats.OverallS += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.S);
            bblUserStats.OverallA += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.A);
            bblUserStats.OverallB += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.B);
            bblUserStats.OverallC += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.C);
            bblUserStats.OverallD += dto.EqAsInt(PlayScoreDto.EPlayScoreMark.D);
            bblUserStats.OverallPerfect += dto.Perfect;
            bblUserStats.OverallHits += dto.GetValue(PlayScoreDto.EPlayScore.Hits);
            bblUserStats.Overall300 += dto.GetValue(PlayScoreDto.EPlayScore.N300);
            bblUserStats.Overall100 += dto.GetValue(PlayScoreDto.EPlayScore.N100);
            bblUserStats.Overall50 += dto.GetValue(PlayScoreDto.EPlayScore.N50);
            bblUserStats.OverallGeki += dto.Geki;
            bblUserStats.OverallKatu += dto.Katu;
            bblUserStats.OverallMiss += dto.Miss;
        }

        return bblUserStats;
    }
}