using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.OdrZip.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Odr;

public class GetOdrZip: FastEndpoints.Endpoint<OdrZipReplayIdDto,
    Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>
> {
    private readonly IOdrZip _odrZip;

    public GetOdrZip(IOdrZip odrZip) {
        _odrZip = odrZip;
    }

    public override void Configure() {
        this.Get("/api2/odr/{replayId:long}.zip");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>> 
        ExecuteAsync(OdrZipReplayIdDto req, CancellationToken ct) {

        var result = await _odrZip.FactoryAsync(req.ReplayId);
        
        if (result == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        if (result.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }
        
        
        
        await this.SendBytesAsync(
            result.Ok().Unwrap().bytes, 
            $"{req.ReplayId}.zip",
            "Application/octet-stream", 
            cancellation: ct
        );
        
        return TypedResults.Ok();
    }
}