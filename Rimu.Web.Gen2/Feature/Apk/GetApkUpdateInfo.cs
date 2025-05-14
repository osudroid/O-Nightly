using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NLog;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Web.Gen2.Interface;
using Logger = NLog.Logger;

namespace Rimu.Web.Gen2.Feature.Apk;

public sealed class GetApkUpdateInfo: FastEndpoints.Endpoint<
    GetApkUpdateInfo.GetApkUpdateInfoRequest, 
    Results<Ok<IResult>, BadRequest<string>, InternalServerError>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvJson _envJson;

    public GetApkUpdateInfo(IEnvJson envJson) {
        _envJson = envJson;
    }

    public override void Configure() {
        this.Get("/api2/apk/version/{dirNameNumber:long}.apk");
        
        AllowAnonymous();
        
    }

    public override Task<Results<Ok<IResult>, BadRequest<string>, InternalServerError>> ExecuteAsync(GetApkUpdateInfoRequest req, CancellationToken ct) {
        try {
            var version = req.DirNameNumber;
            var path = $"{_envJson.UPDATE_PATH}/{version}/android.apk";

            return Task.FromResult<Results<Ok<IResult>, BadRequest<string>, InternalServerError>>(System.IO.File.Exists(path) == false 
                ? TypedResults.BadRequest("Version number not exist") 
                : TypedResults.Ok(Results.File(System.IO.File.OpenRead(path), "application/apk")));
        }
        catch (Exception e) {
            Logger.Error(e);
            return Task.FromResult<Results<Ok<IResult>, BadRequest<string>, InternalServerError>>(TypedResults.InternalServerError());
        }    
    }
    
    public class GetApkUpdateInfoRequest {
        public required long DirNameNumber { get; set; }
    }
}