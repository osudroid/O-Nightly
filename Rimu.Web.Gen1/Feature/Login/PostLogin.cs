using FastEndpoints;
using FluentValidation;
using LamLibAllOver.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Rimu.Kernel;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Avatar.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Pp.Adapter;
using Rimu.Repository.Security.Adapter.Interface;
using Rimu.Web.Gen1.PreProcessor;

namespace Rimu.Web.Gen1.Feature.Login;

// /api/game/login.php

public class PostLogin : FastEndpoints.Endpoint<
    PostLogin.PostLoginRequest,
    Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>
> {
    private readonly PostLoginHandler _postLoginHandler;

    public PostLogin(PostLoginHandler postLoginHandler) {
        _postLoginHandler = postLoginHandler;
    }

    public override void Configure() {
        this.Post("/api/login.php");
        this.PreProcessor<RegionPreProcessor<PostLogin.PostLoginRequest>>();
        this.AllowAnonymous();
    }

    public override async Task<Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>> ExecuteAsync(
        PostLogin.PostLoginRequest req,
        CancellationToken ct) {

        return await this._postLoginHandler.HandleAsync(req, ct);
    }

    public sealed class PostLoginHandler : WebRequestHandler<
        PostLogin.PostLoginRequest,
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>
    > {
        private readonly IQueryUserInfo _queryUserInfo;
        private readonly IQueryUserStats _queryUserStats;
        private readonly IAuthenticationProvider _authenticationProvider;
        private readonly ISecurityPhp _securityPhp;
        private readonly IInputCheckerAndConvertPhp _inputCheckerAndConvertPhp;
        private readonly IEnvDb _envDb;

        public PostLoginHandler(
            HttpContext httpContext,
            IQueryUserInfo queryUserInfo,
            IQueryUserStats queryUserStats,
            IAuthenticationProvider authenticationProvider,
            ISecurityPhp securityPhp,
            IInputCheckerAndConvertPhp inputCheckerAndConvertPhp,
            IEnvDb envDb) : base(httpContext) {
            
            _queryUserInfo = queryUserInfo;
            _queryUserStats = queryUserStats;
            _authenticationProvider = authenticationProvider;
            _securityPhp = securityPhp;
            _inputCheckerAndConvertPhp = inputCheckerAndConvertPhp;
            _envDb = envDb;
        }
        private string CreateAvatarUrl(long userId) => $"https://{_envDb.Domain_Name}/user/avatar?id={userId}.png";
        
        public override async Task<Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError>> HandleAsync(
            PostLoginRequest req,
            CancellationToken ct) {

            var regionPreProcessorState = this.ProcessorState<RegionPreProcessorState>();

            var country = regionPreProcessorState.Country.Unwrap();
            var iPAddress = regionPreProcessorState.IPAddress.Unwrap();

            req.Username = req.Username.Trim();
            req.Password = _inputCheckerAndConvertPhp.PhpStripsLashes(req.Password.Trim());
            
            var version = int.Parse(req.VersionStr);
            if (version <= 42 || req.Sign.Length < 1) {
                return TypedResults.BadRequest("FAIL\nPlease update your client");
            }

            // Check signature
            if (req.Username.Length < 2 || req.Password.Length != 32) {
                return TypedResults.BadRequest("FAIL\nInvalid parameters");
            }


            var authContextResult = await _authenticationProvider.GetUserAuthContextByUsername(req.Username);
            if (authContextResult == EResult.Err) {
                return TypedResults.InternalServerError();
            }


            if (authContextResult.Ok().IsNotSet()) {
                return TypedResults.Ok("FAIL\nWrong name or password");
            }

            var authContext = authContextResult.Ok().Unwrap();

            if (!authContext.Rule.Login) {
                return authContext.Rule switch {
                    { IsBanned: true } => TypedResults.Ok("FAIL\nAccount is bannend"),
                    { IsArchived: true } => TypedResults.Ok("FAIL\nAccount is archived"),
                    _ => TypedResults.Ok("FAIL\nAccount is _"),
                };
            }

            if (!authContext.PasswordGen1EqualHash(req.Password)) {
                return TypedResults.Ok("FAIL\nWrong name or password");
            }

            var userAuthDataContext = authContext.UserDataContext.Unwrap();



            var userInfoRes = await _queryUserInfo.GetByUserIdAsync(userAuthDataContext.UserId);
            if (userInfoRes == EResult.Err || userInfoRes.Ok().IsNotSet()) {
                return TypedResults.InternalServerError();
            }

            var userInfo = userInfoRes.Ok().Unwrap();



            var ssid = _securityPhp.EncryptString(userInfo.UserId.ToString());
            var rank = (await _queryUserStats.GetUserRank(userInfo.UserId)).Ok();
            var stats = (await _queryUserStats.GetBblUserStatsByUserIdAsync(userInfo.UserId)).Ok().Unwrap();

            var stringResult =
                $"SUCCESS\n{CreateAvatarUrl(userInfo.UserId)} {userInfo.UserId} {ssid} {rank} {stats.OverallScore} {stats.OverallPp} {stats.OverallAccuracy} {userInfo.Username}";

            return TypedResults.Ok(stringResult);
        }
        
}
    
    public sealed class PostLoginRequest {
        private string _username = "";
        private string _passwordMd5 = "";
        private string _versionStr = "";
        private string _sign = "";

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "username")]
        public string Username {
            get => _username;
            set => _username = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "password")]
        public string Password {
            get => _passwordMd5;
            set => _passwordMd5 = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "version")]
        public string VersionStr {
            get => _versionStr;
            set => _versionStr = value.Trim();
        }

        [Microsoft.AspNetCore.Mvc.FromForm(Name = "sign")]
        public string Sign {
            get => _sign;
            set => _sign = value.Trim();
        }
    }

    public sealed class PostLoginRequestValidator : Validator<PostLoginRequest> {
        public PostLoginRequestValidator() {
            RuleFor(x => x.VersionStr)
                .Must(x => x.All(char.IsDigit))
                ;
        }
    }
}