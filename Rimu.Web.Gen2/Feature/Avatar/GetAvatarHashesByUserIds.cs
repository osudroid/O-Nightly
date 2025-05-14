using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NLog;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.Feature.Avatar;

public class GetAvatarHashesByUserIds: FastEndpoints.Endpoint<
    GetAvatarHashesByUserIds.AvatarHashesByUserIdsRequest, 
    Results<Ok<GetAvatarHashesByUserIds.AvatarHashesResponse>, BadRequest<string>, InternalServerError, NotFound>
> {
    private new static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly IEnvDb _envDb;
    private readonly IEnvJson _envJson;
    private readonly IQueryUserAvatar _queryUserAvatar;

    public GetAvatarHashesByUserIds(IEnvDb envDb, IEnvJson envJson, IQueryUserAvatar queryUserAvatar) {
        _envDb = envDb;
        _envJson = envJson;
        _queryUserAvatar = queryUserAvatar;
    }

    public override void Configure() {
        this.Post("/api2/avatar/hash");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<AvatarHashesResponse>, BadRequest<string>, InternalServerError, NotFound>> ExecuteAsync(AvatarHashesByUserIdsRequest request, CancellationToken ct) {
        var settingsDb = _envDb;
        
        var size = settingsDb.UserAvatar_SizeLow >= request.Size
            ? settingsDb.UserAvatar_SizeLow
            : settingsDb.UserAvatar_SizeHigh;

        var resp = (await _queryUserAvatar.GetManyUserIdAndHashByPixelSizeUserIdAsync(request.Size, request.UserIds))
            .Map(x => new AvatarHashesResponse() {
                    List = x.Select(v => new AvatarHashesResponse.AvatarHash() {
                                    Hash = v.Hash,
                                    UserId = v.UserId
                                }
                            )
                            .ToList()
                }
            );

        if (resp == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        return TypedResults.Ok(resp.Ok());
    }

    public class AvatarHashesByUserIdsRequest {
        public int Size { get; set; }
        public long[] UserIds { get; set; } = [];
    }
    
    public class AvatarHashesByUserIdsValidator : Validator<AvatarHashesByUserIdsRequest> {
        public AvatarHashesByUserIdsValidator() {
            
        }
    }
    
    public class AvatarHashesResponse {
        public required List<AvatarHash> List { get; set; }
        
        public class AvatarHash {
            public long UserId { get; set; }
            public string? Hash { get; set; }
        }
    }
}