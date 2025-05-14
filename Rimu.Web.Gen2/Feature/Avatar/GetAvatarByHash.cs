using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Avatar;

public class GetAvatarByHash: FastEndpoints.Endpoint<
    GetAvatarByHash.GetAvatarByHashRequest, 
    Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvDb _envDb;
    private readonly IEnvJson _envJson;
    private readonly IQueryUserAvatar _queryUserAvatar;

    public GetAvatarByHash(IEnvDb envDb, IEnvJson envJson, IQueryUserAvatar queryUserAvatar) {
        _envDb = envDb;
        _envJson = envJson;
        _queryUserAvatar = queryUserAvatar;
    }
    
    public override void Configure() {
        Get("/api2/avatar/hash");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>> 
        ExecuteAsync(GetAvatarByHashRequest req, CancellationToken ct) {
        var imageResult = await _queryUserAvatar.GetByHashAsync(req.Hash);

        if (imageResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }


        var imageOpt = imageResult.Ok();
        if (imageOpt.IsNotSet()) {
            TypedResults.NotFound();
        }

        var image = imageOpt.Unwrap();

        return TypedResults.Ok(Results.File(image.Bytes!, image.TypeExt));
    }


    public class GetAvatarByHashRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "hash")]
        public required string Hash { get; set; }
    }
    
    public class GetAvatarByHashRequestValidator : Validator<GetAvatarByHashRequest> {
        public GetAvatarByHashRequestValidator() {
            RuleFor(x => x.Hash)
                .MinimumLength(4);
        }
    }
}