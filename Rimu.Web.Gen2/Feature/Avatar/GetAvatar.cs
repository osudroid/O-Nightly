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

public class GetAvatar: FastEndpoints.Endpoint<
    GetAvatar.GetAvatarRequest, 
    Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvDb _envDb;
    private readonly IEnvJson _envJson;
    private readonly IQueryUserAvatar _queryUserAvatar;

    public GetAvatar(IEnvDb envDb, IEnvJson envJson, IQueryUserAvatar queryUserAvatar) {
        _envDb = envDb;
        _envJson = envJson;
        _queryUserAvatar = queryUserAvatar;
    }

    public override void Configure() {
        Get("/api2/avatar/id");
        AllowAnonymous();
    }

    
    
    public override async Task<Results<Ok<IResult>, BadRequest<string>, InternalServerError, NotFound>> 
        ExecuteAsync(GetAvatarRequest request, CancellationToken ct) {
        var userAvatar_SizeLow = _envDb.UserAvatar_SizeLow;
        
        var imageResult = userAvatar_SizeLow >= request.Size
            ? await _queryUserAvatar.GetLowByUserIdAsync(request.Id)
            : await _queryUserAvatar.GetHighByUserIdAsync(request.Id);

        if (imageResult == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        var imageOpt = imageResult.Ok();
        if (imageOpt.IsNotSet()) {
            return TypedResults.NotFound();
        }

        var image = imageOpt.Unwrap();

        return TypedResults.Ok(Results.File(image.Bytes!, image.TypeExt));
    }

    public class GetAvatarRequest {
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "Size")]
        public required int Size { get; set; }
        [Microsoft.AspNetCore.Mvc.FromQuery(Name = "Id")]
        public required long Id { get; set; }
    }

    public class GetAvatarValidator : Validator<GetAvatarRequest> {
        public GetAvatarValidator() {
            RuleFor(x => x.Id)
                .GreaterThanOrEqualTo(0);
            
            RuleFor(x => x.Size)
                .GreaterThanOrEqualTo(32)
                .LessThanOrEqualTo(2048);
        }
    }
}