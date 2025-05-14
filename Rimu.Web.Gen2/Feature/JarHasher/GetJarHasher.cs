using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.JarHasher;

public class GetJarHasher: FastEndpoints.Endpoint<
    GetJarHasher.GetJarHasherRequest, 
    Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvDb _envDb;
    private readonly IEnvJson _envJson;

    public GetJarHasher(IEnvDb envDb, IEnvJson envJson) {
        _envDb = envDb;
        _envJson = envJson;
    }

    public override void Configure() {
        Get("api2/jar-hasher");
        this.AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<GetJarHasher.GetJarHasherRequest>>();
    }

    public override Task<Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>> ExecuteAsync(GetJarHasherRequest req, CancellationToken ct) {
        var userId= this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap().UserId;
        
        var path = $"{_envJson.JAR_PATH}/{req.Version}.jar";
        if (System.IO.File.Exists(path) == false) {
            Logger.Info($"File was not found in {path}");
            return Task.FromResult<Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>>(TypedResults.NotFound());
        }
        
        Logger.Info("Sending file");
        return Task.FromResult<Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>>(TypedResults.Ok(Results.File(System.IO.File.OpenRead(path), "application/apk")));
    }

    public sealed class GetJarHasherRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "v")] public string Version { get; set; } = "";

        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "q")] public string KeyToken { get; set; } = "";
    }

    public class GetJarHasherValidation : Validator<GetJarHasherRequest> {
        public GetJarHasherValidation(IEnvDb envDb) {
            RuleFor(x => x.Version)
                .NotEmpty()
                .MinimumLength(1)
            ;
            RuleFor(x => x.KeyToken)
                .NotEmpty()
                .MinimumLength(1)
                .Equal(x => envDb.RequestHash_Keyword)
            ;
        }
    }
}