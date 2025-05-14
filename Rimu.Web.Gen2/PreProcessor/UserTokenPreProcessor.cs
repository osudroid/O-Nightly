using FastEndpoints;
using LamLibAllOver.ErrorHandling;
using NLog;
using Rimu.Repository.Authentication.Adapter.Dto;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;

namespace Rimu.Web.Gen2.PreProcessor;

public sealed class UserTokenPreProcessor<T>: PreProcessor<T, UserTokenPreProcessorState> {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IApi2TokenProvider _tokenProvider;
    private readonly IQueryUserInfo _queryUserInfo;
    
    public UserTokenPreProcessor(IApi2TokenProvider tokenProvider, IQueryUserInfo queryUserInfo) {
        _tokenProvider = tokenProvider;
        _queryUserInfo = queryUserInfo;
    }

    public override async Task PreProcessAsync(IPreProcessorContext<T> context, UserTokenPreProcessorState state, CancellationToken ct) {
        var response = context.HttpContext.Response;
        var authToken = context.HttpContext.Request.Headers.Authorization;
        var token = authToken.ToString();

        state.TokenWithTTLDto = Option<TokenWithTTLDto>.Empty;
        
        var tokenWithTTLResult = await _tokenProvider.FindAndIsValidAsync(token);
        if (tokenWithTTLResult == EResult.Err) {
            await response.SendErrorsAsync(context.ValidationFailures, 500, cancellation: ct);
            return;
        }
        if (tokenWithTTLResult.Ok().IsNotSet()) {
            await response.SendErrorsAsync(context.ValidationFailures, 401, cancellation: ct);
            return;
        }

        var tokenWithTTL = tokenWithTTLResult.Ok().Unwrap();
        UserInfo userInfo;
        {
            var userInfoOption = (await _queryUserInfo.GetByUserIdAsync(tokenWithTTL.UserId)).OkOrDefault();
            if (userInfoOption.IsNotSet()) {
                Logger.Error($"User {tokenWithTTL.UserId} not found");
                await response.SendErrorsAsync(context.ValidationFailures, 500, cancellation: ct);
                return;
            }

            userInfo = userInfoOption.Unwrap();
        }
        

        
        var diff = TimeSpan.FromTicks(tokenWithTTL.DeadTimeUtc.Ticks - tokenWithTTL.CreateTime.Ticks);

        if (tokenWithTTL.Ttl / 2 < diff) {
            state.TokenWithTTLDto = Option<TokenWithTTLDto>.With(tokenWithTTL);
            return;
        }

        {
            var tokenWithTTLRefreshResult = await _tokenProvider.UpdateTokenAndUpdateInDbAsync(tokenWithTTL);
            if (tokenWithTTLRefreshResult == EResult.Err) {
                await response.SendErrorsAsync(context.ValidationFailures, 500, cancellation: ct);
                return;
            }

            tokenWithTTL = tokenWithTTLRefreshResult.Ok();
        }

        state.TokenWithTTLDto = Option<TokenWithTTLDto>.With(tokenWithTTL);
    }
}