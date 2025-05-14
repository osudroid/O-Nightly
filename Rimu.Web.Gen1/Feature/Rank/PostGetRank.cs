using System.Diagnostics.CodeAnalysis;
using System.Text;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Rimu.Kernel;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Security.Adapter.Interface;


namespace Rimu.Web.Gen1.Feature.Rank;


// /api/game/leaderboard
public class PostGetRank : FastEndpoints.Endpoint<
    PostGetRank.PostGetRankRequest,
    Results<Ok<string>, NotFound, BadRequest, InternalServerError>
> {
    private readonly PostGetRankHandler _handler;
    
    public PostGetRank(PostGetRankHandler postGetRankHandler) {
        _handler = postGetRankHandler;
    }

    public override void Configure() {
        this.Post("/api/getrank.php");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError>>
        ExecuteAsync(PostGetRankRequest req, CancellationToken ct) {

        return await _handler.HandleAsync(req, ct);
    }



    private enum EOrderBy {
        Pp,
        Score
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class PostGetRankRequest {
        [FromForm(Name = "playID")] public string? PlayId { get; set; }
        [FromForm(Name = "sign")] public string? Sign { get; set; }
        [FromForm(Name = "hash")] public string? Hash { get; set; }
        [FromForm(Name = "uid")] public string? UidStr { get; set; }
        [FromForm(Name = "type")] public string? Type { get; set; }
    }

    public sealed class PostGetRankHandler : WebRequestHandler<
        PostGetRankRequest,
        Results<Ok<string>, NotFound, BadRequest, InternalServerError>
    > {
        // ReSharper disable once UnusedMember.Local
        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IQueryView_Play_PlayStats _queryViewPlayPlayStats;

        // ReSharper disable once InconsistentNaming
        private readonly IQueryView_Play_PlayStats_UserInfo _queryView_Play_PlayStats_UserInfo;
        private readonly ISecurityPhp _securityPhp;
        private readonly IEnvDb _envDb;
        
        public PostGetRankHandler(
            HttpContext httpContext,
            IQueryView_Play_PlayStats queryViewPlayPlayStats,
            IQueryView_Play_PlayStats_UserInfo queryViewPlayPlayStatsUserInfo,
            ISecurityPhp securityPhp,
            IEnvDb envDb
            ) : base(httpContext) {
            _queryViewPlayPlayStats = queryViewPlayPlayStats;
            _queryView_Play_PlayStats_UserInfo = queryViewPlayPlayStatsUserInfo;
            _securityPhp = securityPhp;
            _envDb = envDb;
        }


        public override async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError>> HandleAsync(
            PostGetRankRequest req,
            CancellationToken ct) {
            if (req.Sign is null) {
                return TypedResults.BadRequest();
            }

            if (req.Hash is null) {
                if (req.PlayId is null) {
                    return TypedResults.BadRequest();
                }

                if (!long.TryParse(req.PlayId, out var playId)) {
                    return TypedResults.BadRequest();
                }

                return await this.ByPlayIdAsync(playId, req.Sign.Trim());
            }

            if (req.PlayId is null) {
                return TypedResults.BadRequest();
            }

            if (req.Type is null) {
                return TypedResults.BadRequest();
            }

            if (!long.TryParse(req.UidStr, out var userId)) {
                return TypedResults.BadRequest();
            }

            Option<EOrderBy> orderByOption = req.Type switch {
                "pp" => Option<EOrderBy>.With(EOrderBy.Pp),
                "score" => Option<EOrderBy>.With(EOrderBy.Score),
                _ => default,
            };


            return await ByHashAsync(req.Hash.Trim(), userId, orderByOption.Unwrap(), req.Sign.Trim());
        }

        private async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError>> ByPlayIdAsync(
            long playId,
            string sign) {
            if (!this._securityPhp.CheckRequest(sign, [playId.ToString()])) {
                return TypedResults.BadRequest();
            }

            var viewPlayPlayStatsResult = await _queryViewPlayPlayStats.GetByIdAsync(playId);
            if (viewPlayPlayStatsResult == EResult.Err) {
                return TypedResults.InternalServerError();
            }

            if (viewPlayPlayStatsResult.Ok().IsNotSet()) {
                return TypedResults.NotFound();
            }

            var viewPlayPlayStats = viewPlayPlayStatsResult.Ok().Unwrap();

            var p = viewPlayPlayStats;

            return TypedResults.Ok(
                $"{p.Filename} {p.FileHash} {p.ToDto().ModeAsSingleString()} {p.Score} {p.Combo} {p.Mark} {p.Geki} {p.Perfect} {p.Katu} {p.Good} {p.Bad} {p.Miss} {p.Accuracy} {p.Date:yyyy-M-d hh:mm:ss} {p.Pp}\n"
            );
        }

        private async Task<Results<Ok<string>, NotFound, BadRequest, InternalServerError>> ByHashAsync(
            string hash,
            long userId,
            EOrderBy orderBy,
            string sign) {

            if (!_securityPhp.CheckRequest(sign, [
                        hash,
                        userId.ToString(),
                        orderBy switch {
                            EOrderBy.Pp => "pp",
                            EOrderBy.Score => "score",
                            _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
                        }
                    ])) {
                return TypedResults.BadRequest();
            }

            const int limit = 50;

            var viewPlayPlayStatsUserInfos = await (orderBy switch {
                EOrderBy.Pp => _queryView_Play_PlayStats_UserInfo.GetTopPpByHashAsync(hash, limit),
                EOrderBy.Score => _queryView_Play_PlayStats_UserInfo.GetTopScoreByHashAsync(hash, limit),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
            });

            if (viewPlayPlayStatsUserInfos == EResult.Err) {
                return TypedResults.InternalServerError();
            }


            var resultString = new StringBuilder(512);

            foreach (var viewPlayPlayStatsUserInfo in viewPlayPlayStatsUserInfos.Ok()) {
                var v = viewPlayPlayStatsUserInfo;

                var mode = Rimu.Repository.Postgres.Adapter.Dto.PlayPlayStatsDto.CreateWith(v).ModeAsSingleString();

                var str = $"{v.Id} "
                          + $"{v.Username} "
                          + $"{v.Score} "
                          + $"{v.Pp} "
                          + $"{v.Combo} "
                          + $"{v.Mark} "
                          + $"{mode} "
                          + $"{v.Accuracy} "
                          + $"{CreateAvatarUrl(v.UserId)}"
                          + $"\n";
                resultString.Append(str);
            }

            if (viewPlayPlayStatsUserInfos.Ok().Any(x => x.UserId == userId)) {
                return TypedResults.Ok(resultString.ToString());
            }

            var userBestViewPlayPlayStatsUserInfoResult = await (orderBy switch {
                EOrderBy.Pp => _queryView_Play_PlayStats_UserInfo.GetTopPpByHashAndUserIdAsync(hash, userId),
                EOrderBy.Score => _queryView_Play_PlayStats_UserInfo.GetTopScoreByHashAndUserIdAsync(hash, userId),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
            });

            var rankResult = await (orderBy switch {
                EOrderBy.Pp => _queryView_Play_PlayStats_UserInfo.GetRankSortPpByHashAndUserIdAsync(hash, userId),
                EOrderBy.Score => _queryView_Play_PlayStats_UserInfo.GetRankScoreSortByHashAndUserIdAsync(hash, userId),
                _ => throw new ArgumentOutOfRangeException(nameof(orderBy), orderBy, null)
            });

            if (userBestViewPlayPlayStatsUserInfoResult == EResult.Err
                || rankResult == EResult.Err
                || rankResult.Ok().IsNotSet()) {
                return TypedResults.InternalServerError();
            }

            if (userBestViewPlayPlayStatsUserInfoResult.Ok().IsNotSet()) {
                return TypedResults.Ok(resultString.ToString());
            }

            {
                var uv = userBestViewPlayPlayStatsUserInfoResult.Ok().Unwrap();
                var rank = rankResult.Ok().Unwrap();

                var mode = Rimu.Repository.Postgres.Adapter.Dto.PlayPlayStatsDto.CreateWith(uv).ModeAsSingleString();

                var str = $"{uv.Id} "
                          + $"a "
                          + $"{uv.Score} "
                          + $"{uv.Pp} "
                          + $"{uv.Combo} "
                          + $"{uv.Mark} "
                          + $"{mode} "
                          + $"{uv.Accuracy} "
                          + $"{CreateAvatarUrl(uv.UserId)} "
                          + $"{rank} "
                          + $"\n";
                resultString.Append(str);
            }

            return TypedResults.Ok(resultString.ToString());
        }
        
        private string CreateAvatarUrl(long userId) => $"https://{_envDb.Domain_Name}/user/avatar?id={userId}.png";
    }
}