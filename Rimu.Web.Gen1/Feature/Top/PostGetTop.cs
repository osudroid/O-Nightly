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
using Rimu.Repository.Pp.Adapter;
using Rimu.Repository.Security.Adapter.Interface;
using Rimu.Web.Gen1.PreProcessor;

namespace Rimu.Web.Gen1.Feature.Top;

public sealed class PostGetTop: FastEndpoints.Endpoint<
    PostGetTop.PostGetTopRequest, 
    Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>
> {
    private static readonly new NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly PostGetTopHandler _postGetTopHandler;
    
    public PostGetTop(PostGetTopHandler postGetTopHandler) {
        _postGetTopHandler = postGetTopHandler;
    }

    public override void Configure() {
        this.Post("/api/gettop.php");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>> ExecuteAsync(PostGetTopRequest req, CancellationToken ct) {
       return await _postGetTopHandler.HandleAsync(req, ct);
    }

    
    public sealed class PostGetTopHandler: WebRequestHandler<
        PostGetTop.PostGetTopRequest, 
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>
    > {
        private static readonly NLog.ILogger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IQueryView_Play_PlayStats _queryViewPlayPlayStats;
        private readonly ISecurityPhp _securityPhp;
        private readonly IInputCheckerAndConvertPhp _inputCheckerAndConvertPhp;

        public PostGetTopHandler(HttpContext httpContext, IQueryView_Play_PlayStats queryViewPlayPlayStats, ISecurityPhp securityPhp, IInputCheckerAndConvertPhp inputCheckerAndConvertPhp): base(httpContext) {
            _queryViewPlayPlayStats = queryViewPlayPlayStats;
            _securityPhp = securityPhp;
            _inputCheckerAndConvertPhp = inputCheckerAndConvertPhp;
        }
        
        public override async Task<Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>> HandleAsync(
            PostGetTopRequest req, 
            CancellationToken ct) {

            if (req.Sign is null) { 
                return TypedResults.InternalServerError();
            } 
            if (req.PlayId is null) { 
                if (req.Filename is null || req.Hash is null) { 
                    return TypedResults.BadRequest(""); 
                }

                req.Filename = _inputCheckerAndConvertPhp.FilterBlankStr(req.Filename);
                var filehash = req.Hash.Trim();
            
                if (!_securityPhp.CheckRequest(req.Sign, [req.Filename, req.Hash])) {
                    return TypedResults.InternalServerError();
                }

                var topRank = await _queryViewPlayPlayStats
                    .MapTopPlaysByFilenameAndHashAsync(req.Filename, filehash, 1000);
                if (topRank == EResult.Err) { 
                    Logger.Error("Try GetBeatmapTop");
                }

                if (topRank.Ok().Count == 0) {
                    return TypedResults.Ok("");
                }

                var str = "";
                
                foreach (var play in topRank.Ok()) {
                    str += $"{play.PlayScoreId} {play.Username} {play.Score} {play.Combo} {play.Mark}\n";
                }

                return TypedResults.Ok(str);
            }
        
            req.PlayId = req.PlayId.Trim();
                
            if (!_securityPhp.CheckRequest(req.Sign, [req.PlayId])) { 
                return TypedResults.InternalServerError();
            }

            var playScoreResult = await _queryViewPlayPlayStats.GetByIdAsync(long.Parse(req.PlayId.Trim()));
            if (playScoreResult == EResult.Err) {
                Logger.Error("Try GetByIdAsync");
                return TypedResults.InternalServerError();
            }

            if (playScoreResult.Ok().IsNotSet()) {
                return TypedResults.Ok("\n");
            }
            
            var p = playScoreResult.Ok().Unwrap();
            
            return TypedResults.Ok($"{p.ToDto().ModeAsSingleString()} {p.Score} {p.Combo} {p.Mark} {p.Geki} {p.Perfect} {p.Katu} {p.Good} {p.Bad} {p.Miss} {p.Accuracy} {p.Date:yyyy-M-d hh:mm:ss}");
        }
    }
    
    
    public sealed class PostGetTopRequest {
                [FromForm(Name = "playID")] public string? PlayId { get; set; }
                [FromForm(Name = "sign")] public string? Sign { get; set; }
                [FromForm(Name = "filename")] public string? Filename { get; set; }
                [FromForm(Name = "hash")] public string? Hash { get; set; }
    }
}