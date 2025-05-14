using System.Data;
using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Mailer.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Enum;
using Rimu.Repository.Postgres.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Web.Gen2.PreProcessor;
using Rimu.Web.Gen2.Rule;
using Rimu.Web.Gen2.Share.Signup;

namespace Rimu.Web.Gen2.Feature.Signup;

public sealed class PostSubmit: Endpoint<
    PostSubmit.SubmitRequest, 
    Results<Ok, InternalServerError, NotFound>
> {
    
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly IMailerProvider _mailerProvider;
    private readonly IQueryTokenWithGroup _queryTokenWithGroup;
    private readonly IDbTransactionContext _dbTransactionContext;
    private readonly IEnvDb _envDb;

    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryUserStats _queryUserStats;
    private readonly IQueryUserSetting _queryUserSetting;
    private readonly IQueryUserAvatar _queryUserAvatar;
    private readonly IQueryUserClassifications _queryUserClassifications;

    public PostSubmit(IAuthenticationProvider authenticationProvider, IMailerProvider mailerProvider, IQueryTokenWithGroup queryTokenWithGroup, IDbTransactionContext dbTransactionContext, IEnvDb envDb, IQueryUserInfo queryUserInfo, IQueryUserStats queryUserStats, IQueryUserSetting queryUserSetting, IQueryUserAvatar queryUserAvatar, IQueryUserClassifications queryUserClassifications) {
        _authenticationProvider = authenticationProvider;
        _mailerProvider = mailerProvider;
        _queryTokenWithGroup = queryTokenWithGroup;
        _dbTransactionContext = dbTransactionContext;
        _envDb = envDb;
        _queryUserInfo = queryUserInfo;
        _queryUserStats = queryUserStats;
        _queryUserSetting = queryUserSetting;
        _queryUserAvatar = queryUserAvatar;
        _queryUserClassifications = queryUserClassifications;
    }

    public override void Configure() {
        Post("/api2/signup/signup");
        this.AllowAnonymous();
        this.PreProcessor<RegionPreProcessor<PostSubmit.SubmitRequest>>();
    }

    public override async Task<Results<Ok, InternalServerError, NotFound>> 
        ExecuteAsync(SubmitRequest req, CancellationToken ct) {
        var iPAddress =  this.ProcessorState<RegionPreProcessorState>().IPAddress.Unwrap();
        var country =  this.ProcessorState<RegionPreProcessorState>().Country.Unwrap();
        _dbTransactionContext.SetIsolationLevel(IsolationLevel.Serializable);
        if (await _dbTransactionContext.BeginTransactionAsync() == EResult.Err) {
            return TypedResults.InternalServerError();
        }

        var tokenWithGroupDataDtoResult = 
            (await _queryTokenWithGroup.FindByTokenGroupAndTokenAsync(ETokenGroup.SIGNUP_ACCOUNT, req.Token))
            .Map(x => 
                x.IsSet() 
                && x.Unwrap().CreateTime.Add(_envDb.TokenSignup_TTL) < DateTime.UtcNow ? default : x)
            .Map(static x => x
                .Map(static x => TokenWithGroupDataDto.FromJson(x.Data)))
            .AndThen(static x => x.Match(
                    static () => ResultOk<Option<TokenWithGroupDataDto>>.Ok(default),
                    static res => res.Match(
                        static () => ResultOk<Option<TokenWithGroupDataDto>>.Err(),
                        static v => ResultOk<Option<TokenWithGroupDataDto>>.Ok(Option<TokenWithGroupDataDto>.NullSplit(v)
                        )
                    )
                )
            );
        
        if (tokenWithGroupDataDtoResult == EResult.Err || tokenWithGroupDataDtoResult.Ok().IsNotSet()) {
            await _dbTransactionContext.RollbackAsync();
            
            return tokenWithGroupDataDtoResult == EResult.Err 
                ? TypedResults.InternalServerError() 
                : TypedResults.NotFound()
            ;
        }

        var tokenWithGroupDataDto = tokenWithGroupDataDtoResult.Ok().Unwrap();

        var userIdResult = await _queryUserInfo.GetNewUserIdAsync();
        if (userIdResult == EResult.Err) {
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }

        var userId = userIdResult.Ok();
        
        var userInfo = new UserInfo() {
            UserId = userId,
            Username = req.Username,
            Password = _authenticationProvider.PasswordGen1Provider.HashPassword(req.Password),
            PasswordGen2 = _authenticationProvider.PasswordGen2Provider.HashPassword(req.Password),
            Email = req.Email,
            DeviceId = "",
            RegisterTime = DateTime.UtcNow,
            LastLoginTime = DateTime.UtcNow,
            Region = country.NameShort,
            Active = true,
            Banned = false,
            RestrictMode = false,
            UsernameLastChange = DateTime.MinValue,
            LatestIp = iPAddress.ToString(),
            PatronEmail = null,
            PatronEmailAccept = false,
            Archived = false
        };

        var userClassifications = new UserClassifications() {
            Contributor = false,
            CoreDeveloper = false,
            Developer = false,
            Supporter = false,
            UserId = userId, 
        };

        var userSetting = new UserSetting() {
            UserId = userId,
            ShowUserClassifications = true
        };

        var userStats = new UserStats() {
            UserId = userId,
            OverallPlaycount = 0,
            OverallScore = 0,
            OverallAccuracy = 0,
            OverallCombo = 0,
            OverallXss = 0,
            OverallSs = 0,
            OverallXs = 0,
            OverallS = 0,
            OverallA = 0,
            OverallB = 0,
            OverallC = 0,
            OverallD = 0,
            OverallPerfect = 0,
            OverallHits = 0,
            Overall300 = 0,
            Overall100 = 0,
            Overall50 = 0,
            OverallGeki = 0,
            OverallKatu = 0,
            OverallMiss = 0,
            OverallPp = 0,
        };

        if (await _queryUserInfo.InsertAsync(userInfo) == EResult.Err
            || await _queryUserClassifications.InsertAsync(userClassifications) == EResult.Err
            || await _queryUserSetting.InsertAsync(userSetting) == EResult.Err
            || await _queryUserStats.InsertAsync(userStats) == EResult.Err
            || await _dbTransactionContext.CommitAsync() == EResult.Err) {
            
            await _dbTransactionContext.RollbackAsync();
            return TypedResults.InternalServerError();
        }
        
        return TypedResults.Ok();
        
    }

    public sealed class SubmitRequest {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string Token { get; set; } = "";

        public sealed class SubmitRequestValidator : Validator<SubmitRequest> {
            public SubmitRequestValidator() {
                this
                    .UseRuleUsername(request => request.Username)
                    .UseRuleEmail(request => request.Email)
                    .UseRulePassword(request => request.Password)
                    .UseRuleHex(request => request.Token)
                    ;
            }
        }
    }
}