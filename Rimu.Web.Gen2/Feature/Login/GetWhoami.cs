using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;

namespace Rimu.Web.Gen2.Feature.Login;

public class GetWhoami: FastEndpoints.EndpointWithoutRequest<
    Results<Ok<GetWhoami.WhoamiDto>, BadRequest, InternalServerError, UnauthorizedHttpResult>
> {
    private readonly IQueryUserInfo _queryUserInfo;

    public GetWhoami(IQueryUserInfo queryUserInfo) {
        _queryUserInfo = queryUserInfo;
    }

    public override void Configure() {
        Patch("/api/login/whomai");
        AllowAnonymous();
        this.PreProcessor<UserTokenPreProcessor<EmptyRequest>>();
    }

    public override async Task<Results<Ok<WhoamiDto>, BadRequest, InternalServerError, UnauthorizedHttpResult>> 
        ExecuteAsync(CancellationToken ct) {

        var tokenWithTtlDto = this.ProcessorState<UserTokenPreProcessorState>().TokenWithTTLDto.Unwrap();
        var whoamiDtoOption = (await _queryUserInfo.GetByUserIdAsync(tokenWithTtlDto.UserId)).Map(x => {
            return x.Map(x => new WhoamiDto {
                    UserId = x.UserId,
                    Username = x.Username ?? throw new NullReferenceException(nameof(UserInfo.Username)),
                    Email = x.Email ?? throw new NullReferenceException(nameof(UserInfo.Email)),
                    Region = x.Region ?? throw new NullReferenceException(nameof(UserInfo.Region)),
                }
            );
        }).OkOrDefault();

        if (whoamiDtoOption.IsNotSet()) {
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok(whoamiDtoOption.Unwrap());
    }

    public sealed class WhoamiDto {
        public required long UserId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Region { get; set; }
    } 
}