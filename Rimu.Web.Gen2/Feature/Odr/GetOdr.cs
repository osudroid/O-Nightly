using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Odr;

public class GetOdr: FastEndpoints.Endpoint<GetOdr.OdrReplayId,
    Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>
> {
    private readonly IQueryReplayFile _queryReplayFile;

    public GetOdr(IQueryReplayFile queryReplayFile) {
        _queryReplayFile = queryReplayFile;
    }

    public override void Configure() {
        this.Get("/api2/odr/{replayId:long}.odr");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>> 
        ExecuteAsync(OdrReplayId req, CancellationToken ct) {

        var result = await this._queryReplayFile.GetByIdAsync(req.ReplayId);
        if (result == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        if (result.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }
        
        await this.SendBytesAsync(result.Ok().Unwrap().Odr, $"{req.ReplayId}.odr", "Application/octet-stream", cancellation: ct);
        return TypedResults.Ok();
    }


    public sealed class OdrReplayId {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "replayId")]
        public long ReplayId { get; set; }
    }
    
    public sealed class OdrReplayIdValidator : Validator<OdrReplayId> {
        public OdrReplayIdValidator() {
            RuleFor(x => x.ReplayId)
                .GreaterThanOrEqualTo(0)
            ;
        }
    }
}