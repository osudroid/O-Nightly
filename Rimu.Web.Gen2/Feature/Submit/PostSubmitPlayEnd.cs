using System.Data;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Pp.Adapter;
using Rimu.Web.Gen2.Interface;
using Rimu.Web.Gen2.PreProcessor;
using Rimu.Web.Gen2.Share.Submit;

namespace Rimu.Web.Gen2.Feature.Submit;

public class PostSubmitPlayEnd: FastEndpoints.Endpoint<HashWithDataRequest<PostSubmitPlayEnd.SubmitPlayEnd>,
    Results<Ok<PostSubmitPlayEnd.SubmitPlayEndResponse>, BadRequest, InternalServerError, NotFound>
> {
    private readonly IQueryPlayStats _queryPlayStats;
    private readonly IQueryUserStats _queryUserStats;
    private readonly IPpCalculatorContext _ppCalculatorContext;
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;
    private readonly IQueryPlayStatsHistory _queryPlayStatsHistory;
    private readonly IQueryPlay _queryPlay;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly IQueryReplayFile _queryReplayFile;

    public PostSubmitPlayEnd(IQueryPlayStats queryPlayStats, IQueryUserStats queryUserStats, IPpCalculatorContext ppCalculatorContext, IQueryView_Play_PlayStats queryViewPlayPlayStats, IQueryPlayStatsHistory queryPlayStatsHistory, IQueryPlay queryPlay, IDbTransactionContext dbTransactionContext, IQueryReplayFile queryReplayFile) {
        _queryPlayStats = queryPlayStats;
        _queryUserStats = queryUserStats;
        _ppCalculatorContext = ppCalculatorContext;
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
        _queryPlayStatsHistory = queryPlayStatsHistory;
        _queryPlay = queryPlay;
        _dbTransactionContext = dbTransactionContext;
        _queryReplayFile = queryReplayFile;
    }


    public override void Configure() {
        Post("/api2/submit/play-end");
        AllowAnonymous();
        AllowFileUploads();
        PreProcessor<UserTokenPreProcessor<HashWithDataRequest<SubmitPlayEnd>>>();
        PreProcessor<RequestHashValidationPreProcessor<HashWithDataRequest<SubmitPlayEnd>,SubmitPlayEnd>>();
    }

    public override async Task<Results<Ok<SubmitPlayEndResponse>, BadRequest, InternalServerError, NotFound>> 
        ExecuteAsync(HashWithDataRequest<SubmitPlayEnd> req, CancellationToken ct) {
        
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;
        var data = req.Data!;

        var playResult = await _queryPlay.GetByIdAsync(data.Id);
        if (playResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        if (playResult.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }

        var play = playResult.Ok().Unwrap();

        var topPlayResult = await _queryView_Play_PlayStats.GetUserTopPpAsync(userId, play.Filename, play.FileHash);
        if (topPlayResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Snapshot);
        await _dbTransactionContext.BeginTransactionAsync();
        
        if (topPlayResult.Ok().IsSet()) {
            if (topPlayResult.Ok().Unwrap().Pp >= data.Pp) {
                await _dbTransactionContext.CommitAsync();
                
                return TypedResults.Ok(new SubmitPlayEndResponse {
                    GlobalRank = -1,
                    MapRank = -1,
                    NewBestPlay = false,
                    PlayPlayStats = null,
                });
            }

            if (await _queryPlayStatsHistory
                    .InsertWithNewIdAsync(PlayStatsHistory.From(topPlayResult.Ok().Unwrap())) == EResult.Err) {
                
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }

            await _queryPlayStats.DeleteByIdAsync(topPlayResult.Ok().Unwrap().Id);
        }
        
        var replayFileIdResult = await InsertNewReplayFileAndValidatedPpAsync(data);
        if (replayFileIdResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        long replayFileId = replayFileIdResult.Ok(); 
        
        if (await _queryPlayStats.InsertAsync(new PlayStats() {
                Id = data.Id,
                Mode = PlayMode.ModeAsSingleStringToModeArray(data.Mode),
                Score = data.Score,
                Combo = data.Combo,
                Mark = data.Mark,
                Geki = data.Geki,
                Perfect = data.Perfect,
                Katu = data.Katu,
                Good = data.Good,
                Bad = data.Bad,
                Miss = data.Miss,
                Date = DateTime.UtcNow,
                Accuracy = data.Accuracy,
                Pp = data.Pp,
                ReplayFileId = replayFileId,
            }) == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        var viewPlayPlayStatsNewResult = await UpdateUserStatsAndReturnNewPlayAsync(topPlayResult.Ok().Unwrap());
        if (viewPlayPlayStatsNewResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        var userRankGlobalResult = await _queryUserStats.GetUserRank(userId);
        var mapRankResult = await _queryView_Play_PlayStats.GetUserMapRankAsync(data.Id);
        if (userRankGlobalResult == EResult.Err || mapRankResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        if (await _dbTransactionContext.CommitAsync() == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        
        return TypedResults.Ok(new SubmitPlayEndResponse {
                NewBestPlay = true,
                GlobalRank = userRankGlobalResult.Ok(),
                MapRank = mapRankResult.Ok(),
                PlayPlayStats = viewPlayPlayStatsNewResult.Ok().ToDto(),
            }
        );
    }

    private async Task<ResultOk<long>> InsertNewReplayFileAndValidatedPpAsync(SubmitPlayEnd data) {
        byte[] replayFileBytes = Array.Empty<byte>();
        await using var mem = new MemoryStream();
        await using var stream = Files[0].OpenReadStream();
        await stream.CopyToAsync(mem);
        replayFileBytes = mem.ToArray();

        var calculateResult = await _ppCalculatorContext.CalculateReplayAsync(replayFileBytes, data.File!.FileName);
        if (calculateResult == EResult.Err) {
            return ResultOk<long>.Err();
        }

        if (calculateResult.Ok().Unwrap() < data.Pp) {
            return ResultOk<long>.Err();
        }
            
        var replayFileIdResult = await _queryReplayFile.InsertAsync(replayFileBytes);
        if (replayFileIdResult == EResult.Err) {
            return ResultOk<long>.Err();
        }

        return ResultOk<long>.Ok(replayFileIdResult.Ok());
    }

    private async Task<ResultOk<View_Play_PlayStats>> UpdateUserStatsAndReturnNewPlayAsync(View_Play_PlayStats old) {
        var userId = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;
        
        var newViewPlayPlayStatsResult = await _queryView_Play_PlayStats.GetByIdAsync(old.Id);
            
        if (newViewPlayPlayStatsResult == EResult.Err
            || newViewPlayPlayStatsResult.Ok().IsNotSet()
            || newViewPlayPlayStatsResult == EResult.Err) {

            return ResultOk<View_Play_PlayStats>.Err();
        }

        var playStatsDtoOld = PlayStatsDto.Create(old);
        var playStatsDtoNew = PlayStatsDto.Create(newViewPlayPlayStatsResult.Ok().Unwrap().ToPlayStats());

        if (await _queryUserStats.UpdateStatsFromPlayStatsAsync(userId, playStatsDtoNew, playStatsDtoOld) == EResult.Err) {
            return ResultOk<View_Play_PlayStats>.Err();
        }

        return ResultOk<View_Play_PlayStats>.Ok(newViewPlayPlayStatsResult.Ok().Unwrap());
    }
    

    public sealed class SubmitPlayEnd: ISingleString {
        [FromForm] public IFormFile? File { get; set; }
        
        [FromForm] public long Id { get; set; }
        [FromForm] public string Mode { get; set; } = "";
        [FromForm] public long Score { get; set; }
        [FromForm] public long Combo { get; set; }
        [FromForm] public string Mark { get; set; } = "";
        [FromForm] public long Geki { get; set; }
        [FromForm] public long Perfect { get; set; }
        [FromForm] public long Katu { get; set; }
        [FromForm] public long Good { get; set; }
        [FromForm] public long Bad { get; set; }
        [FromForm] public long Miss { get; set; }
        [FromForm] public double Accuracy { get; set; }
        [FromForm] public double Pp { get; set;  }
        
        public string ToSingleString() {
            return LamLibAllOver.Merge.ListToString([
                    Id.ToString(),
                    Mode.ToString(),
                    Score.ToString(),
                    Combo.ToString(),
                    Mark.ToString(),
                    Geki.ToString(),
                    Perfect.ToString(),
                    Katu.ToString(),
                    Good.ToString(),
                    Bad.ToString(),
                    Miss.ToString(),
                    Accuracy.ToString("F1"),
                    Pp.ToString("F1"),
            ]);
        }
    }
    
    public sealed class SubmitPlayEndResponse {
        public bool NewBestPlay { get; set; }
        public PlayPlayStatsDto? PlayPlayStats { get; set; }
        public long MapRank { get; set; }
        public long GlobalRank { get; set; }
    }
}