using System.Collections.Concurrent;
using NLog;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Postgres.Domain.Query;
using Logger = NLog.Logger;

namespace Rimu.Terminal.Provider;

internal class CalcUserScoreProvider {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IQueryUserStats _queryUserStats;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;

    public CalcUserScoreProvider(
        IQueryUserStats queryUserStats, 
        IQueryUserInfo queryUserInfo, 
        IQueryView_Play_PlayStats queryViewPlayPlayStats) {
        
        _queryUserStats = queryUserStats;
        _queryUserInfo = queryUserInfo;
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
    }

    public async Task RunAsync() {
        try {
            Logger.Info("Run CalcUserScoreHandler");

            List<UserStats> userStatsList;
            {
                Logger.Info("Run CalcUserScoreHandler GetAllPlayScores");
                var playScoreList = RemoveAllDtosWith0Pp(await GetAllPlayPlayStatsAsDto());
                
                GC.Collect();

                Logger.Info("Run CalcUserScoreHandler GetAllUserIds");
                var userInfoList = await GetAllUserInfoIdsAsync();
                
                Logger.Info("Run CalcUserScoreHandler CalcStats");
                userStatsList = CalcUserStats(playScoreList, userInfoList);
            }

            Logger.Debug("User Count With   PP: {}", userStatsList.Count(x => x.OverallPp > 0));
            Logger.Debug("User Count With 0 PP: {}", userStatsList.Count(x => x.OverallPp == 0));
            
            Logger.Info("Run CalcUserScoreHandler Insert");
            GC.Collect();
            
            await InsertOrUpdateUserStatsAsync(userStatsList.ToArray());
        }
        catch (Exception e) {
            Logger.Error(e);
            await Task.Delay(1000);
            throw;
        }
    }
    
    /// <exception cref="Exception"></exception>
    private async Task<List<View_Play_PlayStats_Dto>> GetAllPlayPlayStatsAsDto() {
        var listSResult = await _queryView_Play_PlayStats.GetAllAsync();
        if (listSResult == EResult.Err) {
            throw new Exception("Run CalcUserScoreHandler GetAllPlayScores");
        }
        var listResult = new List<View_Play_PlayStats_Dto>(listSResult.Ok().Count);
        
        foreach (var playPlayStats in listSResult.Ok()) {
            var dto = View_Play_PlayStats_Dto.From(playPlayStats);
            listResult.Add(dto);
        }

        return listResult;
    }

    /// <exception cref="Exception"></exception>
    private async Task<List<UserInfo>> GetAllUserInfoIdsAsync() {
        return (await _queryUserInfo.GetAllAsync()).Ok();
    }

    private List<UserStats> CalcUserStats(
        List<View_Play_PlayStats_Dto> playScoreList,
        List<UserInfo> userInfoList) {
        Dictionary<long, ConcurrentBag<PlayStatsDto>> dic = new(userInfoList.Count);

        foreach (var userInfo in userInfoList) dic.Add(userInfo.UserId, new ConcurrentBag<PlayStatsDto>());

        Logger.Debug("Fill Dictionary With PlayScores");
        Parallel.ForEach(
            playScoreList,
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            v => {
                if (!dic.TryGetValue(v.Play.UserId, out var bag))
                    return;

                bag.Add(v.PlayStats);
            }
        );
        GC.Collect();
        
        var userStatsBag = new ConcurrentBag<UserStats>();

        Logger.Debug("Calc");
        Parallel.ForEach(
            dic.ToList(),
            new ParallelOptions { MaxDegreeOfParallelism = 16 },
            pair => {
                userStatsBag.Add(CreateUserStats(pair.Key, pair.Value.ToArray()));
            }
        );

        return userStatsBag.ToList();
    }

    /// <exception cref="Exception"></exception>
    private async Task InsertOrUpdateUserStatsAsync(UserStats[] userStatsArr) {
        foreach (var userStatss in userStatsArr.Chunk(10_000)) {
            var err = await _queryUserStats.InsertOrUpdateBulkAsync(userStatss);
            if (err == EResult.Err) {
                Logger.Error("Run InsertUserStats Failed");
                throw new Exception("Run InsertUserStats Failed");
            }    
        }
    }

    private UserStats CreateUserStats(long userId, PlayStatsDto[] playStatsDtoList) {
        var bblUserStats = new UserStats {
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
            OverallMiss = 0,
            OverallPp = 0,
        };

        if (playStatsDtoList.Length == 0) {
            // Logger.Info($"UserId: {userId} Has Not Plays");
            return bblUserStats;
        }
        
        bblUserStats.OverallPlaycount = playStatsDtoList.Length;
        foreach (var dto in playStatsDtoList) {
            bblUserStats.OverallScore += dto.Score;
            bblUserStats.OverallAccuracy += dto.Accuracy;
            bblUserStats.OverallCombo += dto.Combo;
            bblUserStats.OverallXss += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.XSS);
            bblUserStats.OverallSs += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.SS);
            bblUserStats.OverallXs += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.XS);
            bblUserStats.OverallS += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.S);
            bblUserStats.OverallA += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.A);
            bblUserStats.OverallB += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.B);
            bblUserStats.OverallC += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.C);
            bblUserStats.OverallD += dto.MarkEqualAsInt(PlayStatsDto.EPlayScoreMark.D);
            bblUserStats.OverallPerfect += dto.Perfect;
            bblUserStats.OverallHits += (long)dto.GetValue(PlayStatsDto.EPlayScore.Hits);
            bblUserStats.Overall300 += (long)dto.GetValue(PlayStatsDto.EPlayScore.N300);
            bblUserStats.Overall100 += (long)dto.GetValue(PlayStatsDto.EPlayScore.N100);
            bblUserStats.Overall50 += (long)dto.GetValue(PlayStatsDto.EPlayScore.N50);
            bblUserStats.OverallGeki += dto.Geki;
            bblUserStats.OverallKatu += dto.Katu;
            bblUserStats.OverallMiss += dto.Miss;
            bblUserStats.OverallPp += dto.Pp;
        }

        return bblUserStats;
    }

    private List<View_Play_PlayStats_Dto> RemoveAllDtosWith0Pp(List<View_Play_PlayStats_Dto> dtos) {
        List<View_Play_PlayStats_Dto> resultList = new(dtos.Count);

        foreach (var dto in dtos) {
            if (dto.PlayStats.Pp == 0) {
                continue;
            }
            resultList.Add(dto);
        }
        
        return resultList;
    }
}