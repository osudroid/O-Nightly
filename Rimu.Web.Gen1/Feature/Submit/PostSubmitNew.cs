using System.Data;
using System.Security.Cryptography;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NLog;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Dto;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Pp.Adapter;
using Rimu.Repository.Security.Adapter.Interface;


namespace Rimu.Web.Gen1.Feature.Submit;

// /api/game/submit.php
public sealed class PostSubmitNew: FastEndpoints.Endpoint<
    PostSubmitNew.PostSubmitNewRequest, 
    Results<Ok<string>, NotFound<string>, BadRequest<string>, InternalServerError>
> {
    private new static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly IInputCheckerAndConvertPhp _inputCheckerAndConvertPhp;
    private readonly IPpCalculatorContext _ppCalculatorContext;
    private readonly IQueryPlay _queryPlay;
    private readonly IQueryPlayStats _queryPlayStats;
    private readonly IQueryPlayStatsHistory _queryPlayStatsHistory;
    private readonly IQueryReplayFile _queryReplayFile;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryUserStats _queryUserStats;
    private readonly IQueryView_Play_PlayStats _queryView_Play_PlayStats;
    private readonly ISecurityPhp _securityPhp;
    private readonly IServiceProvider _serviceProvider;

    public PostSubmitNew(IAuthenticationProvider authenticationProvider, IDbTransactionContext dbTransactionContext, IInputCheckerAndConvertPhp inputCheckerAndConvertPhp, IPpCalculatorContext ppCalculatorContext, IQueryPlay queryPlay, IQueryPlayStats queryPlayStats, IQueryPlayStatsHistory queryPlayStatsHistory, IQueryReplayFile queryReplayFile, IQueryUserInfo queryUserInfo, IQueryUserStats queryUserStats, IQueryView_Play_PlayStats queryViewPlayPlayStats, ISecurityPhp securityPhp, IServiceProvider serviceProvider) {
        _authenticationProvider = authenticationProvider;
        _dbTransactionContext = dbTransactionContext;
        _inputCheckerAndConvertPhp = inputCheckerAndConvertPhp;
        _ppCalculatorContext = ppCalculatorContext;
        _queryPlay = queryPlay;
        _queryPlayStats = queryPlayStats;
        _queryPlayStatsHistory = queryPlayStatsHistory;
        _queryReplayFile = queryReplayFile;
        _queryUserInfo = queryUserInfo;
        _queryUserStats = queryUserStats;
        _queryView_Play_PlayStats = queryViewPlayPlayStats;
        _securityPhp = securityPhp;
        _serviceProvider = serviceProvider;
    }

    public override void Configure() {
        this.Post("/api/submit.php");
        this.AllowFileUploads();
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>, InternalServerError>> ExecuteAsync(PostSubmitNewRequest req, CancellationToken ct) {
        byte[] fileBytes;
        await using (var stream = req.ReplayFile!.OpenReadStream()) {
            await using var mem = new MemoryStream();
            await stream.CopyToAsync(mem, ct);
            fileBytes = mem.ToArray();
        }

        if (!_securityPhp.DecodeString(req.UserIdStr, req.Ssid)) {
            return TypedResults.BadRequest("FAIL\nCannot decode signature");
        }
        
        var userId = long.Parse(req.UserIdStr);

        var userAuthContextResult = await this._authenticationProvider.GetUserAuthContextByUserId(userId);
        if (userAuthContextResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        if (userAuthContextResult.Ok().IsNotSet()) {
            return TypedResults.NotFound("FAIL\nUID does not exist");
        }

        if (!userAuthContextResult.Ok().Unwrap().Rule.ScoreSubmission) {
            return TypedResults.BadRequest("FAIL\nYou are currently restricted.");
        }

        if (!_securityPhp.CheckRequest(req.Sign, [
                req.UserIdStr,
                req.Ssid,
                req.Filename,
                req.Hash,
                req.DataStr,
                req.ReplayFileChecksum,
            ])) {
            return TypedResults.BadRequest("FAIL\nInvalid signature");
        }

        {
            var inputHash = SHA256.HashData(fileBytes);
            var hex = Convert.ToHexString(inputHash);
            if (hex != req.ReplayFileChecksum) {
                return TypedResults.BadRequest("FAIL\nThe replay file has been tampered");
            }
        }
        
        var dataArr = req.DataStr.Split(' ');

        if (dataArr.Length < 11) {
            return TypedResults.BadRequest("FAIL\nInvalid data");
        }
        
        var ppResult = await _ppCalculatorContext.CalculateReplayAsync(fileBytes, req.Filename);
        if (ppResult == EResult.Err) {
            Logger.Error("Calculate PP but Error: {}", ppResult.Err().ToString());
            return TypedResults.InternalServerError();
        }
        
        if (ppResult.Ok().IsNotSet()) {
            return TypedResults.BadRequest("FAIL\nInvalid File");
        }
        
        var playScoreNew = new View_Play_PlayStats() {
            Id = -1,
            UserId = userId,
            Filename = req.Filename,
            FileHash = req.Hash,
            Mode = PlayMode.ModeAsSingleStringToModeArray(dataArr[0]),
            Score = long.Parse(dataArr[1]),
            Combo = long.Parse(dataArr[2]),
            Mark = dataArr[3],
            Geki = long.Parse(dataArr[4]),
            Perfect = long.Parse(dataArr[5]),
            Katu = long.Parse(dataArr[6]),
            Good = long.Parse(dataArr[7]),
            Bad = long.Parse(dataArr[8]),
            Miss = long.Parse(dataArr[9]),
            Date = DateTime.UtcNow,
            Accuracy = long.Parse(dataArr[10]),
            Pp = ppResult.Ok().Unwrap(),
            ReplayFileId = null
        };
        
        if (playScoreNew.Score > 1500000000) {
            return TypedResults.BadRequest("FAIL\nScore exceeded limit");
        }

        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Snapshot);
        if (await this._dbTransactionContext.BeginTransactionAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        
        var idResult = (await _queryPlay.InsertIfNotExistAsync(playScoreNew.ToPlay()));
        if (idResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        playScoreNew.Id = idResult.Ok();
        
        var topPlayOldResult = await _queryView_Play_PlayStats.GetUserTopPpAsync(userId, playScoreNew.Filename, playScoreNew.FileHash);
        if (topPlayOldResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        if (topPlayOldResult.Ok().IsSet()) {
            var topPlayOld = topPlayOldResult.Ok().Unwrap();

            if (topPlayOld.IsBetterThen(playScoreNew)) {
                var result = await this.CreateOkResponseMessageAsync(topPlayOld);
                if (result == EResult.Err) {
                    await _dbTransactionContext.RollbackAsync();
                    return TypedResults.InternalServerError();
                }
                return result.Ok();
            }
            
            if (await _queryPlayStatsHistory
                    .InsertWithNewIdAsync(PlayStatsHistory.From(topPlayOldResult.Ok().Unwrap())) == EResult.Err) {
                
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }

            await _queryPlayStats.DeleteByIdAsync(topPlayOldResult.Ok().Unwrap().Id);
        }
        
        {
            var replayFileIdResult = await _queryReplayFile.InsertAsync(fileBytes);
            if (replayFileIdResult == EResult.Err) {
                await _dbTransactionContext.RollbackAsync();
                return TypedResults.InternalServerError();
            }
        
            playScoreNew.ReplayFileId = replayFileIdResult.Ok();
        }
        
        var resultNone = await UpdateUserStatsAsync(topPlayOldResult.Ok(), playScoreNew);
        if (resultNone == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        
        if (await _queryPlayStats.InsertAsync(playScoreNew.ToPlayStats()) == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        var resultDataResult = await CreateOkResponseMessageAsync(playScoreNew);
        if (resultDataResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        return resultDataResult.Ok();
    }

    private async Task<ResultOk<Ok<string>>> CreateOkResponseMessageAsync(View_Play_PlayStats viewPlayPlayStats) {
        var userStatsResult = await _queryUserStats.GetByUserIdAsync(viewPlayPlayStats.UserId);
        var userRankResult = await _queryUserStats.GetUserRank(viewPlayPlayStats.UserId);
        var userMapRankResult = await _queryView_Play_PlayStats.GetUserMapRankAsync(viewPlayPlayStats.Id);
        if (userStatsResult == EResult.Err || userStatsResult.Ok().IsNotSet() || userRankResult == EResult.Err || userMapRankResult == EResult.Err) {
            return ResultOk<Ok<string>>.Err();
        }

        var userStats = userStatsResult.Ok().Unwrap();
        var userRank = userRankResult.Ok();
        var userMapRank = userMapRankResult.Ok();
        
        return ResultOk<Ok<string>>.Ok(TypedResults.Ok($"SUCCESS\n{userRank} {userStats.OverallScore} {userStats.OverallAccuracy} {userMapRank} {userStats.OverallPp}"));
    }

    private async Task<ResultNone> UpdateUserStatsAsync(
        Option<View_Play_PlayStats> oldPlayerPlayStats,
        View_Play_PlayStats newPlayerPlayStats) {

        return await _queryUserStats.UpdateStatsFromPlayStatsAsync(
            newPlayerPlayStats.UserId,
            PlayStatsDto.Create(newPlayerPlayStats),
                oldPlayerPlayStats.Map(PlayStatsDto.Create).OrNull()
        );
    }


    public sealed class PostSubmitNewRequest {
        private string _userIdStr = "";
        private string _ssid = "";
        private string _filename = "";
        private string _hash = "";
        private string _dataStr = "";
        private string _replayFileChecksum = "";
        private string _sign = "";

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "userID")]
        public string UserIdStr {
            get => _userIdStr;
            set => _userIdStr = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "ssid")]
        public string Ssid {
            get => _ssid;
            set => _ssid = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "filename")]
        public string Filename {
            get => _filename;
            set => _filename = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "hash")]
        public string Hash {
            get => _hash;
            set => _hash = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "data")]
        public string DataStr {
            get => _dataStr;
            set => _dataStr = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "replayFileChecksum")]
        public string ReplayFileChecksum {
            get => _replayFileChecksum;
            set => _replayFileChecksum = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "replayFile")] 
        public IFormFile?  ReplayFile { get; set; } // TODO Schauen ob es der richtige name ist

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "sign")]
        public string Sign {
            get => _sign;
            set => _sign = value.Trim();
        }
    }

    public sealed class PostSubmitNewRequestValidator : Validator<PostSubmitNewRequest> {
        public PostSubmitNewRequestValidator() {
            RuleFor(x => x.UserIdStr)
                .MinimumLength(1);
            RuleFor(x => x.Ssid)
                .MinimumLength(1);
            RuleFor(x => x.Hash)
                .MinimumLength(4);
            RuleFor(x => x.DataStr)
                .MinimumLength(1);
            RuleFor(x => x.ReplayFileChecksum)
                .MinimumLength(4);
            RuleFor(x => x.ReplayFile)
                .NotNull()
                .Must(x => x!.Length > 10);
            RuleFor(x => x.Sign)
                .MinimumLength(1);
        }
    }
}