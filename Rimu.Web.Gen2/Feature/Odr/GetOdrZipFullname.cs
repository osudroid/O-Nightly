using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Rimu.Repository.OdrZip.Adapter.Interface;

namespace Rimu.Web.Gen2.Feature.Odr;

public sealed class GetOdrZipFullname: FastEndpoints.Endpoint<GetOdrZipFullname.OdrZipReplayIdAndFullname,
    Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>
> {
    private readonly IOdrZip _odrZip;

    public GetOdrZipFullname(IOdrZip odrZip) {
        _odrZip = odrZip;
    }

    public override void Configure() {
        this.Get("/api2/odr/fullname/{replayId:long}/{fullname}.zip");
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok, BadRequest, InternalServerError, UnauthorizedHttpResult, NotFound>> 
        ExecuteAsync(GetOdrZipFullname.OdrZipReplayIdAndFullname req, CancellationToken ct) {

        var result = await _odrZip.FactoryAsync(req.ReplayId);
        
        if (result == EResult.Err) {
            return TypedResults.InternalServerError();
        }
        if (result.Ok().IsNotSet()) {
            return TypedResults.NotFound();
        }
        
        
        
        await this.SendBytesAsync(
            result.Ok().Unwrap().bytes, 
            $"{req}.zip",
            "Application/octet-stream", 
            cancellation: ct
        );
        
        return TypedResults.Ok();
    }


    public sealed class OdrZipReplayIdAndFullname {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "replayId")] public long ReplayId { get; set; }

        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "fullname")] public string Fullname { get; set; } = "";
    }
    
    public sealed class OdrZipRReplayIdAndFullnameValidator : Validator<OdrZipReplayIdAndFullname> {
        public OdrZipRReplayIdAndFullnameValidator() {
            RuleFor(x => x.ReplayId)
                .GreaterThanOrEqualTo(0)
                ;
            RuleFor(x => x.Fullname)
                .MinimumLength(1)
            ;
        }
    }
}