using System.Net;
using FakeItEasy;
using FastEndpoints;
using FluentAssertions;
using LamLibAllOver.ErrorHandling;
using MaxMind.GeoIP2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using Rimu.Repository.Authentication.Adapter.Interface;
using Rimu.Repository.Environment.Adapter.Interface;
using Rimu.Repository.Postgres.Adapter.Entities;
using Rimu.Repository.Postgres.Adapter.Query;
using Rimu.Repository.Region.Adapter.Interface;
using Rimu.Repository.Security.Adapter.Interface;
using Rimu.Web.Gen1.Feature.Login;
using Rimu.Web.Gen1.PreProcessor;
namespace Rimu.Web.Gen1.Test.Unit.Feature.Login;

public sealed class PostLoginTest {
    private readonly HttpContext _httpContext;
    private readonly IQueryUserInfo _queryUserInfo;
    private readonly IQueryUserStats _queryUserStats;
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly ISecurityPhp _securityPhp;
    private readonly IInputCheckerAndConvertPhp _inputCheckerAndConvertPhp;
    private readonly IEnvDb _envDb;
    private readonly PostLogin.PostLoginHandler _handler;
    private readonly long _userId = 1;
    public PostLoginTest() {
        _httpContext = NSubstitute
                       .Substitute
                       .For<HttpContext>();
        _queryUserInfo = NSubstitute.Substitute.For<IQueryUserInfo>();
        _queryUserStats = NSubstitute.Substitute.For<IQueryUserStats>();
        _authenticationProvider = NSubstitute.Substitute.For<IAuthenticationProvider>();
        _securityPhp = NSubstitute.Substitute.For<ISecurityPhp>();
        _inputCheckerAndConvertPhp = NSubstitute.Substitute.For<IInputCheckerAndConvertPhp>();
        _envDb = NSubstitute.Substitute.For<IEnvDb>();
        _handler = new PostLogin.PostLoginHandler(
            _httpContext, 
            _queryUserInfo, 
            _queryUserStats, 
            _authenticationProvider, 
            _securityPhp, 
            _inputCheckerAndConvertPhp,
            _envDb);
    }
    
    
    
    [SetUp]
    public void Setup() {
        var state = new RegionPreProcessorState() {
            Country = Option<ICountry>.With(Substitute.For<ICountry>()),
            IPAddress = Option<IPAddress>.With(IPAddress.Parse("5.5.5.5")),
        };

        _httpContext.Items
                    .TryGetValue("2", out _)
                    .Returns(x => {
                            x[1] = state;
                            return true;
                        }
                    );
    }

    [Test]
    public void GetLogin_Setup() {
        var postLogin = new PostLogin(_handler);

        postLogin.Configure();
    }
    
    [Test]
    public async Task GetLogin_InvalidVersion() {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "",
            Sign = "asdasdas",
            Username = "aasd",
            VersionStr = "3" 
        };
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username);
        
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler.HandleAsync(
            loginRequest, CancellationToken.None);

        
        var result = response.Result;
        var badRequest = result.Should()
              .BeOfType<BadRequest<string>>()
              .Which
              .As<BadRequest<string>>()
              ;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("FAIL\nPlease update your client");
    }
    
    [Test]
    public async Task GetLogin_ValidVersion() {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "",
            Sign = "asd",
            Username = "",
            VersionStr = "54" 
        };
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username);
        
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler.HandleAsync(
            loginRequest, CancellationToken.None);

        
        var result = response.Result;
        var badRequest = result.Should()
                               .BeOfType<BadRequest<string>>()
                               .Which
                               .As<BadRequest<string>>()
            ;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("FAIL\nInvalid parameters");
    }
    
    [Test]
    [TestCase("a")]
    [TestCase("b")]
    [TestCase("b   ")]
    [TestCase("  b  ")]
    public async Task GetLogin_ToShortUsername(string username) {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "",
            Sign = "asdasdasda",
            Username = username,
            VersionStr = "54" 
        };
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username);
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler.HandleAsync(
            loginRequest, CancellationToken.None);

        var result = response.Result;
        var badRequest = result.Should()
                               .BeOfType<BadRequest<string>>()
                               .Which
                               .As<BadRequest<string>>()
            ;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("FAIL\nInvalid parameters");
    }
    
    [Test]
    [TestCase("a")]
    [TestCase("b")]
    [TestCase("b   ")]
    [TestCase("  b  ")]
    [TestCase("  baaaaasdöjkaslödksalödlaskdlöasköldkaslödköaskdölasköldkaslökdlöasködlkasöldköalskdlöaskd  ")]
    public async Task GetLogin_ToShortOrToLongPassword(string password) {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = password,
            Sign = "asdasdasda",
            Username = "asdjaklsjd",
            VersionStr = "54" 
        };
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username);
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler.HandleAsync(
            loginRequest, CancellationToken.None);

        var result = response.Result;
        var badRequest = result.Should()
                               .BeOfType<BadRequest<string>>()
                               .Which
                               .As<BadRequest<string>>()
            ;
        badRequest.StatusCode.Should().Be(400);
        badRequest.Value.Should().Be("FAIL\nInvalid parameters");
    }
    
    [Test]
    public async Task GetLogin_FalseUsername() {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "",
            Sign = "",
            Username = "A",
            VersionStr = "43" 
        };
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username);
        
        
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler.HandleAsync(
            loginRequest, CancellationToken.None);

        var result = response.Result;
        result = response.Result;
    }

    [Test]
    public async Task GetLogin_UserNotFound() {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Sign = "asdasdasda",
            Username = "tomtom",
            VersionStr = "54" 
        };

        _authenticationProvider
            .GetUserAuthContextByUsername(loginRequest.Username)
            .Returns(ResultOk<Option<IUserAuthContext>>.Ok(default))
            ;
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username.Trim());
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Password).Returns(loginRequest.Password.Trim());
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler
            .HandleAsync(loginRequest, CancellationToken.None);

        var result = response.Result;
        var badRequest = result.Should()
                               .BeOfType<Ok<string>>()
                               .Which
                               .As<Ok<string>>()
            ;
        badRequest.StatusCode.Should().Be(200);
        badRequest.Value.Should().Be("FAIL\nWrong name or password");
    }
    
    [Test]
    [TestCase("FAIL\nAccount is bannend", 0)]
    [TestCase("FAIL\nAccount is archived", 1)]
    [TestCase("FAIL\nAccount is _", 2)]
    public async Task GetLogin_HasNotTheRule(string responseString, int checkCount) {
        var userRule = NSubstitute.Substitute.For<IUserRule>();
        userRule.Login.Returns(false);
        
        switch (checkCount) {
            case 0:
                userRule.IsBanned.Returns(true);
                break;
            case 1:
                userRule.IsBanned.Returns(false);
                userRule.IsArchived.Returns(true);
                break;
            case 2:
                userRule.IsBanned.Returns(false);
                userRule.IsArchived.Returns(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(checkCount), "only 0,1,2 are valid numbers");
        }
        
        var userAuthContext = NSubstitute.Substitute.For<IUserAuthContext>();
        userAuthContext.Rule.Returns(userRule);
        
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Sign = "asdasdasda",
            Username = "tomtom",
            VersionStr = "54" 
        };

        _authenticationProvider
            .GetUserAuthContextByUsername(loginRequest.Username)
            .Returns(ResultOk<Option<IUserAuthContext>>.Ok(Option<IUserAuthContext>.With(userAuthContext)))
            ;
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username.Trim());
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Password).Returns(loginRequest.Password.Trim());
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler
            .HandleAsync(loginRequest, CancellationToken.None);

        var result = response.Result;
        var badRequest = result.Should()
                               .BeOfType<Ok<string>>()
                               .Which
                               .As<Ok<string>>()
            ;
        badRequest.StatusCode.Should().Be(200);
        badRequest.Value.Should().Be(responseString);
    }
    
    [Test]
    public async Task GetLogin_FalsePasswordHash() {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Sign = "asdasdasda",
            Username = "tomtom",
            VersionStr = "54" 
        };
        
        var userRule = NSubstitute.Substitute.For<IUserRule>();
        userRule.Login.Returns(true);
        
        var userAuthContext = NSubstitute.Substitute.For<IUserAuthContext>();
        userAuthContext.Rule.Returns(userRule);
        userAuthContext.PasswordGen1EqualHash(loginRequest.Password).Returns(false);
        

        _authenticationProvider
            .GetUserAuthContextByUsername(loginRequest.Username)
            .Returns(ResultOk<Option<IUserAuthContext>>.Ok(Option<IUserAuthContext>.With(userAuthContext)))
            ;
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username.Trim());
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Password).Returns(loginRequest.Password.Trim());
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler
            .HandleAsync(loginRequest, CancellationToken.None);

        var result = response.Result;
        var badRequest = result.Should()
                               .BeOfType<Ok<string>>()
                               .Which
                               .As<Ok<string>>()
            ;
        badRequest.StatusCode.Should().Be(200);
        badRequest.Value.Should().Be("FAIL\nWrong name or password");
    }

    [Test]
    public async Task GetLogin_Success() {
        var loginRequest = new PostLogin.PostLoginRequest() {
            Password = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
            Sign = "asdasdasda",
            Username = "tomtom",
            VersionStr = "54" 
        };

        var userAuthDataContext = NSubstitute.Substitute.For<IUserAuthDataContext>();
        userAuthDataContext.UserId.Returns(_userId);
        
        var userRule = NSubstitute.Substitute.For<IUserRule>();
        userRule.Login.Returns(true);
        
        var userAuthContext = NSubstitute.Substitute.For<IUserAuthContext>();
        userAuthContext.Rule.Returns(userRule);
        userAuthContext.PasswordGen1EqualHash(loginRequest.Password).Returns(true);
        userAuthContext.UserDataContext
            .Returns(Option<IUserAuthDataContext>.With(userAuthDataContext));

        _authenticationProvider
            .GetUserAuthContextByUsername(loginRequest.Username)
            .Returns(ResultOk<Option<IUserAuthContext>>.Ok(Option<IUserAuthContext>.With(userAuthContext)))
            ;

        _securityPhp.EncryptString("1").Returns("356a192b7913b04c54574d18c28d46e6395428ab");
        var userStats = new UserStats() {
            UserId = _userId,
            OverallPlaycount = 1,
            OverallScore = 1,
            OverallAccuracy = 1,
            OverallCombo = 1,
            OverallXss = 1,
            OverallSs = 1,
            OverallXs = 1,
            OverallS = 1,
            OverallA = 1,
            OverallB = 1,
            OverallC = 1,
            OverallD = 1,
            OverallPerfect = 1,
            OverallHits = 1,
            Overall300 = 1,
            Overall100 = 1,
            Overall50 = 1,
            OverallGeki = 1,
            OverallKatu = 1,
            OverallMiss = 1,
            OverallPp = 1,
        };
        
        _queryUserStats
            .GetUserRank(_userId)
            .Returns(Task.FromResult(ResultOk<long>.Ok(1)));
        _queryUserStats.GetBblUserStatsByUserIdAsync(_userId)
            .Returns(Task.FromResult(ResultOk<Option<UserStats>>.Ok(Option<UserStats>.With(userStats))));
        
        _queryUserInfo.GetByUserIdAsync(_userId)
            .Returns(ResultOk<Option<UserInfo>>.Ok(Option<UserInfo>.With(new UserInfo() {
                UserId = _userId,
                Username = "",
                Password = "",
                PasswordGen2 = "",
                Email = "",
                DeviceId = "",
                RegisterTime = default,
                LastLoginTime = default,
                Region = "",
                Active = true,
                Banned = false,
                Archived = false,
                RestrictMode = false,
                UsernameLastChange = default,
                LatestIp = "0.0.0.0",
                PatronEmail = null,
                PatronEmailAccept = false,
            })));
        
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Username).Returns(loginRequest.Username.Trim());
        this._inputCheckerAndConvertPhp.PhpStripsLashes(loginRequest.Password).Returns(loginRequest.Password.Trim());
        
        Results<Ok<string>, NotFound, BadRequest<string>, InternalServerError> response = await _handler
            .HandleAsync(loginRequest, CancellationToken.None);

        var result = response.Result;
        var badRequest = result.Should()
                .BeOfType<Ok<string>>()
                .Which
                .As<Ok<string>>()
            ;
        badRequest.StatusCode.Should().Be(200);
        badRequest.Value.Should().Be("SUCCESS\nhttps:///user/avatar?id=1.png 1 356a192b7913b04c54574d18c28d46e6395428ab 1 1 1 1 ");
    }
}